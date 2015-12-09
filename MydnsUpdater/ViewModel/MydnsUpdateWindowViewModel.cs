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

namespace MydnsUpdater.ViewModel
{
    class MydnsUpdateWindowViewModel
    {
        /// <summary>
        /// MydnsのマスターID
        /// </summary>
        [Required(ErrorMessage = "必須入力です")]
        public ReactiveProperty<string> MasterId { get; private set; }

        /// <summary>
        /// Mydnsのパスワード
        /// </summary>
        public ReactiveProperty<string> Password { get; private set; }

        /// <summary>
        /// Mydnsへの更新間隔
        /// </summary>
        [Required(ErrorMessage = "必須入力です")]
        [RegularExpression(@"[0-9]+", ErrorMessage = "半角数字のみ入力できます。")]
        public ReactiveProperty<string> UpdateSpan { get; private set; }

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

        public IObservable<bool> IsProcessing { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MydnsUpdateWindowViewModel()
        {
            // 書くテキストボックスのバリデーションを有効化
            // MasterIdとUpdateSpanはRequiredAttribute検証の有効化。
            // Passwordは独自のバリデーションを設定。
            this.MasterId = new ReactiveProperty<string>()
                .SetValidateAttribute(() => this.MasterId);
            this.Password = new ReactiveProperty<string>()
                .SetValidateNotifyError(x => string.IsNullOrEmpty(x) ? "必須入力です" : null);
            this.UpdateSpan = new ReactiveProperty<string>()
                .SetValidateAttribute(() => this.UpdateSpan);

            // ReactiveCommandの生成
            // 更新コマンドはMasterIdとPasswordが正しく入力がされている場合に限りCommandを受け付ける
            this.DnsUpdateCommand = new[]{
                this.MasterId.ObserveHasErrors,
                this.Password.ObserveHasErrors,
            }
            .CombineLatestValuesAreAllFalse()
            .ToReactiveCommand();

            // インターバルコマンドは全てのテキストボックスが正しく入力がされている
            // かつインターバル実行中でなければCommandを受け付ける
            this.DnsIntervalUpdateCommand = new[]
            {
                this.MasterId.ObserveHasErrors,
                this.Password.ObserveHasErrors,
                this.UpdateSpan.ObserveHasErrors
            }
            .CombineLatestValuesAreAllFalse()
            .ToReactiveCommand();

            // キャンセルコマンドは一定間隔更新コマンドが動いている時のみCommandを受け付ける
            this.DnsCancelIntervalCommand = new[]
            {
                this.IsProcessing
            }
            .CombineLatestValuesAreAllTrue()
            .ToReactiveCommand();


            // 購読開始
            this.DnsUpdateCommand.Subscribe(_ =>
            {
                UpdateMydnsServer();
            });

            // TODO:テスト
            InitializeTestMethod();

        }

        private void UpdateMydnsServer()
        {
            Console.WriteLine("この辺りにそれっぽいメインロジック作る");
        }

        // TODO:テスト
        private void InitializeTestMethod()
        {
            var source = Observable.Timer(TimeSpan.FromSeconds(5),TimeSpan.FromSeconds(1));
            source.Subscribe(_ =>
            {
                UpdateMydnsServer();
            });
        }

    }
}
