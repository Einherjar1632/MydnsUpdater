using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Tracing;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MydnsUpdater.Model;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;

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
        [Required(ErrorMessage = @"必須入力です")]
        [RegularExpression(@"[0-9]+", ErrorMessage = @"半角数字のみ入力できます")]
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
        public ReadOnlyReactiveCollection<DynamicDnsViewModel> ItemsList { get; set; }
        #endregion

        #region AthorMember
        /// <summary>
        /// コマンド実行中かどうかを判定するIObservableなカウンター
        ///   カウンターが0より大きければ実行中
        ///   カウンターが0であれば停止中
        /// </summary>
        private readonly CountNotifier _countNotifer = new CountNotifier(1);

        /// <summary>
        /// 更新系コマンドが実行中であることを表すプロパティ
        /// </summary>
        private IObservable<bool> IsExecuting { get; set; }

        /// <summary>
        /// 実行タイマー
        /// </summary>
        private IObservable<long>  Timer { get; set; }

        private IDisposable TimerCancel { get; set; }

        private ReactiveProperty<bool> IsDnsIntervalUpdateExecuting { get; set; }  = new ReactiveProperty<bool>(false);

        /// <summary>
        /// Modelの情報を保持
        /// </summary>
        private MyDns Model { get; }
        #endregion

        #region constructor
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MydnsUpdateWindowViewModel()
        {
            InitializeValidation();
            InitializeCommand();

            Model = new MyDns();

            ItemsList = Model.ItemsCollection
                .ToReadOnlyReactiveCollection(x => new DynamicDnsViewModel(x));

            // 購読開始(未作成)
            DnsUpdateCommand.Subscribe(_ =>
            {
                UpdateMydnsServer();
            });
            DnsIntervalUpdateCommand.Subscribe( _ =>
            {
                IntervalUpdateMydnsServerAsync();
            });
            DnsCancelIntervalCommand.Subscribe(_ =>
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
            MasterId = new ReactiveProperty<string>()
                .SetValidateNotifyError(x => string.IsNullOrEmpty(x) ? "必須入力です" : null);
            Password = new ReactiveProperty<string>()
                .SetValidateNotifyError(x => string.IsNullOrEmpty(x) ? "必須入力です" : null);
            // UpdateSpanはRequiredAttribute検証の有効化
            UpdateSpan = new ReactiveProperty<string>()
                .SetValidateAttribute(() => UpdateSpan);
        }

        /// <summary>
        /// 各コマンドの生成
        /// </summary>
        private void InitializeCommand()
        {
            // カウンターをIObservable<bool>に変換
            IsExecuting = _countNotifer
                .Select(x => x != CountChangedStatus.Empty) // カウンターが0の時に実行中だと判定する
                .ToReactiveProperty();

            // 更新コマンドはMasterIdとPasswordが正しく入力がされている場合に限りCommandを受け付ける
            DnsUpdateCommand = new[]{
                MasterId.ObserveHasErrors
                , Password.ObserveHasErrors
                , IsExecuting
            }
            .CombineLatestValuesAreAllFalse() // 指定したObserveHasErrorsが全て無くなった時に処理できる
            .ToReactiveCommand();

            // インターバルコマンドは全てのテキストボックスが正しく入力がされている
            // かつインターバル実行中でなければCommandを受け付ける
            DnsIntervalUpdateCommand = new[]
            {
                MasterId.ObserveHasErrors
                , Password.ObserveHasErrors
                , UpdateSpan.ObserveHasErrors
                , IsDnsIntervalUpdateExecuting
            }
            .CombineLatestValuesAreAllFalse() // 指定したObserveHasErrorsが全て無くなった時に処理できる
            .ToReactiveCommand();


            // 更新コマンドが実行可能かどうかを判断するIObservable<bool>を生成
            //  特に必要なIObservableではないが後学のために用意
            var canDnsUpdateCommand = DnsUpdateCommand
                .CanExecuteChangedAsObservable()　// ICommandに実装されているCanExecuteChangedをIObservable<EventArgs>に変換して
                .Select(_ => DnsUpdateCommand.CanExecute()); // IObservable<bool>に変換

            // キャンセルコマンドは更新系コマンド実行中
            // かつ更新コマンドが実行可能な時のみCommandを受け付ける
            DnsCancelIntervalCommand = new[]
            {
                IsDnsIntervalUpdateExecuting
            }
            .CombineLatestValuesAreAllTrue()
            .ToReactiveCommand();
        }
        #endregion


        private  void IntervalUpdateMydnsServerAsync()
        {
            Timer = Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(long.Parse(UpdateSpan.Value)));
            TimerCancel = Timer.Subscribe(async _ =>
            {
                IsDnsIntervalUpdateExecuting.Value = true;
                using (_countNotifer.Increment())
                {
                    await Model.UpdateDnsServerAsync(MasterId.Value, Password.Value);
                }

            });
        }

        /// <summary>
        /// キャンセルさせます
        /// </summary>
        private void CancelIntervalAsync()
        {
            IsDnsIntervalUpdateExecuting.Value = false;
            TimerCancel.Dispose();
            _countNotifer.Decrement();
        }


        /// <summary>
        /// 即時実行します
        /// </summary>
        private async void UpdateMydnsServer()
        {
            using (_countNotifer.Increment())
            {
                await Model.UpdateDnsServerAsync(MasterId.Value, Password.Value);
            }
        }
    }
}
