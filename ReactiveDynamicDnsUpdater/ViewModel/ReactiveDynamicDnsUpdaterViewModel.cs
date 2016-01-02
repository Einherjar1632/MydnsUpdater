using System;
using System.ComponentModel.DataAnnotations;
using System.Reactive.Linq;
using ReactiveDynamicDnsUpdater.Model;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;

namespace ReactiveDynamicDnsUpdater.ViewModel
{
    /// <summary>
    /// メインウインドウのViewModel
    /// </summary>
    class ReactiveDynamicDnsUpdaterViewModel
    {
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
        /// <summary>
        /// DNS更新のコマンド
        /// </summary>
        public ReactiveCommand DnsUpdateCommand { get; private set; }
        /// <summary>
        /// DNSを一定間隔で更新するインターバルコマンド
        /// </summary>
        public ReactiveCommand DnsIntervalUpdateCommand { get; private set; }
        /// <summary>
        /// DNSの一定間隔更新をキャンセルするコマンド
        /// </summary>
        public ReactiveCommand DnsCancelIntervalCommand { get; private set; }
        /// <summary>
        /// DynamicDNSへの更新結果を保持する読み取り専用コレクション
        /// </summary>
        public ReadOnlyReactiveCollection<DynamicDnsViewModel> ItemsList { get; set; }
        /// <summary>
        /// インターバルコマンドが実行中かどうかを判断するフラグ
        /// </summary>
        private ReactiveProperty<bool> IsDnsIntervalUpdateExecuting { get; set; } = new ReactiveProperty<bool>(false);

        /// <summary>
        /// コマンド実行中かどうかを判定するIObservableなカウンター
        ///   カウンターが0より大きければ実行中
        ///   カウンターが0であれば停止中
        ///   IDisposableを実装しているためDecrementは使用せずUsingを推奨
        /// </summary>
        private readonly CountNotifier _countNotifer = new CountNotifier(1);
        /// <summary>
        /// コマンド実行中かどうかを判定するIObservableなプロパティ
        ///   ReactiveCommandのCanExecuteはIObservable＜bool＞を監視するため
        ///   CountNotifierをこのプロパティに変換する
        /// </summary>
        private IObservable<bool> IsExecuting { get; set; }
        /// <summary>
        /// 一定間隔更新用のタイマー
        /// </summary>
        private IObservable<long> Timer { get; set; }
        /// <summary>
        /// タイマーの生存期間を管理する
        /// </summary>
        private IDisposable TimerAvailable { get; set; }

        /// <summary>
        /// Modelの情報を保持
        /// </summary>
        private MyDns Model { get; set; }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ReactiveDynamicDnsUpdaterViewModel()
        {
            InitializeValidation();
            InitializeCommand();
            InitializeCollections();
            InitializeSubscribes();
        }

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

            // キャンセルコマンドは更新系コマンド実行中
            // かつ更新コマンドが実行可能な時のみCommandを受け付ける
            DnsCancelIntervalCommand = new[]
            {
                IsDnsIntervalUpdateExecuting
            }
            .CombineLatestValuesAreAllTrue()
            .ToReactiveCommand();
        }

        /// <summary>
        /// コレクションの初期化
        /// </summary>
        private void InitializeCollections()
        {
            Model = new MyDns();
            ItemsList = Model.ItemsList
                .ToReadOnlyReactiveCollection(x => new DynamicDnsViewModel(x));
        }

        /// <summary>
        /// 購読者を設定
        /// </summary>
        private void InitializeSubscribes()
        {
            DnsUpdateCommand.Subscribe(_ =>
            {
                UpdateMydnsServer();
            });
            DnsIntervalUpdateCommand.Subscribe(_ =>
            {
                IntervalUpdateMydnsServerAsync();
            });
            DnsCancelIntervalCommand.Subscribe(_ =>
            {
                CancelIntervalAsync();
            });
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

        /// <summary>
        /// 定期的に実行します
        /// </summary>
        private  void IntervalUpdateMydnsServerAsync()
        {
            Timer = Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(long.Parse(UpdateSpan.Value)));
            TimerAvailable = Timer.Subscribe(async _ =>
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
            TimerAvailable.Dispose();
            _countNotifer.Decrement();
        }
    }
}
