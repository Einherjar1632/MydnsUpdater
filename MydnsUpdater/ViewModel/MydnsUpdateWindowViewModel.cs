using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using System.ComponentModel.DataAnnotations;
using System.Reactive.Linq;
using MydnsUpdater.Model;

namespace MydnsUpdater.ViewModel
{
    class MydnsUpdateWindowViewModel
    {
        #region ReactiveProperty
        /// <summary>
        /// MydnsのマスターID
        /// </summary>
        public ReactiveProperty<string> MasterId { get; private set; }

        /// <summary>
        /// Mydnsのパスワード
        /// </summary>
        public ReactiveProperty<string> Password { get; private set; }

        /// <summary>
        /// Mydnsへの更新間隔
        /// </summary>
        [Required(ErrorMessage = "必須入力です")]
        [RegularExpression(@"[0-9]+", ErrorMessage = "半角数字のみ入力できます")]
        public ReactiveProperty<string> UpdateSpan { get; private set; }
        #endregion

        #region ReactiveCommand
        /// <summary>
        /// DNS更新のコマンド
        /// </summary>
        public ReactiveCommand DnsUpdateCommand { get; private set; }

        /// <summary>
        /// DNSを一定間隔で更新するコマンド
        /// </summary>
        public ReactiveCommand DnsIntervalUpdateCommand { get; private set; }

        /// <summary>
        /// DNSの一定間隔更新をキャンセルするコマンド
        /// </summary>
        public ReactiveCommand DnsCancelIntervalCommand { get; private set; }
        #endregion

        #region ReadOnlyReactiveCollection
        /// <summary>
        /// DynamicDNSへの更新結果履歴を保持する読み取り専用コレクション
        /// </summary>
        public ReadOnlyReactiveCollection<DynamicDNSResponseViewModel> ItemsList { get; private set; }
        #endregion

        #region AthorMember
        /// <summary>
        /// コマンド実行中かどうかを判定するIObservableなカウンター
        ///   カウンターが0より大きければ実行中
        ///   カウンターが0であれば停止中
        /// </summary>
        private CountNotifier countNotifer = new CountNotifier(1);

        /// <summary>
        /// 更新系コマンドが実行中であることを表すプロパティ
        /// </summary>
        private IObservable<bool> IsExecuting { get; set; }

        private MyDnsDnsHttpAccess Model { get; set; }
        #endregion

        #region constructor
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MydnsUpdateWindowViewModel()
        {
            InitializeValidation();
            InitializeCommand();

            // 購読開始(未作成)
            this.DnsUpdateCommand.Subscribe(_ =>
            {
                UpdateMydnsServer();
            });
            this.DnsIntervalUpdateCommand.Subscribe(async _ =>
            {
                await IntervalUpdateMydnsServerAsync();
            });
            this.DnsCancelIntervalCommand.Subscribe(_ =>
            {
                CancelIntervalAsync();
            });
        }
        #endregion

        #region PrivateMethod
        /// <summary>
        /// バインドされているコマンドのバリデーション設定
        /// </summary>
        private void InitializeValidation()
        {
            // 各テキストボックスのバリデーションを有効化
            // MasterIdとPasswordは独自のバリデーションを設定
            this.MasterId = new ReactiveProperty<string>()
                .SetValidateNotifyError(x => string.IsNullOrEmpty(x) ? "必須入力です" : null);
            this.Password = new ReactiveProperty<string>()
                .SetValidateNotifyError(x => string.IsNullOrEmpty(x) ? "必須入力です" : null);
            // UpdateSpanはRequiredAttribute検証の有効化
            this.UpdateSpan = new ReactiveProperty<string>()
                .SetValidateAttribute(() => this.UpdateSpan);
        }

        /// <summary>
        /// 各コマンドの生成
        /// </summary>
        private void InitializeCommand()
        {
            // 更新コマンドはMasterIdとPasswordが正しく入力がされている場合に限りCommandを受け付ける
            this.DnsUpdateCommand = new[]{
                this.MasterId.ObserveHasErrors
                , this.Password.ObserveHasErrors
            }
            .CombineLatestValuesAreAllFalse() // 指定したObserveHasErrorsが全て無くなった時に処理できる
            .ToReactiveCommand();

            // カウンターをIObservable<bool>に変換
            this.IsExecuting = this.countNotifer
                .Select(x => x != CountChangedStatus.Empty) // カウンターが0の時に実行中だと判定する
                .ToReactiveProperty();

            // インターバルコマンドは全てのテキストボックスが正しく入力がされている
            // かつインターバル実行中でなければCommandを受け付ける
            this.DnsIntervalUpdateCommand = new[]
            {
                this.MasterId.ObserveHasErrors
                , this.Password.ObserveHasErrors
                , this.UpdateSpan.ObserveHasErrors
                , this.IsExecuting
            }
            .CombineLatestValuesAreAllFalse() // 指定したObserveHasErrorsが全て無くなった時に処理できる
            .ToReactiveCommand();


            // 更新コマンドが実行可能かどうかを判断するIObservable<bool>を生成
            //  特に必要なIObservableではないが後学のために用意
            var canDnsUpdateCommand = this.DnsUpdateCommand
                .CanExecuteChangedAsObservable()　// ICommandに実装されているCanExecuteChangedをIObservable<EventArgs>に変換して
                .Select(_ => this.DnsUpdateCommand.CanExecute()); // IObservable<bool>に変換

            // キャンセルコマンドは更新系コマンド実行中
            // かつ更新コマンドが実行可能な時のみCommandを受け付ける
            this.DnsCancelIntervalCommand = new[]
            {
                IsExecuting
                , canDnsUpdateCommand
            }
            .CombineLatestValuesAreAllTrue()
            .ToReactiveCommand();
        }
        #endregion


        private async Task IntervalUpdateMydnsServerAsync()
        {
            using (countNotifer.Increment())
            {
                Model = new MyDnsDnsHttpAccess(MasterId.Value, Password.Value);
                await Model.UpdateDnsServerAsync();
                this.ItemsList = this.Model.ItemsCollection
                    .ToReadOnlyReactiveCollection(x => new DynamicDNSResponseViewModel(x));
                Console.Write("あ");

            }

        }

        private void CancelIntervalAsync()
        {
             countNotifer.Decrement();
        }



        private async void UpdateMydnsServer()
        {
            using (countNotifer.Increment())
            {
                await Task.Delay(5000);
                Console.WriteLine("この辺りにそれっぽいメインロジック作る");
            }
            //countNotifer.Increment();
        }

    }
}
