using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Reactive.Linq;

namespace MydnsUpdater.ViewModel
{
    class MydnsUpdateWindowViewModel
    {
        /// <summary>
        /// MydnsのマスターID
        /// </summary>
        [Required(ErrorMessage = "Error!!")]
        public ReactiveProperty<string> MasterId { get; private set; }

        /// <summary>
        /// Mydnsのパスワード
        /// </summary>
        [Required(ErrorMessage = "Error!!")]
        public ReactiveProperty<string> Password { get; private set; }

        /// <summary>
        /// DNS更新ボタン
        /// </summary>
        public ReactiveCommand DnsUpdateCommand { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MydnsUpdateWindowViewModel()
        {

            // RequiredAttributeの設定
            this.MasterId = new ReactiveProperty<string>()
                .SetValidateAttribute(() => this.MasterId);
            this.Password = new ReactiveProperty<string>()
                .SetValidateAttribute(() => this.Password);

            // ReactiveCommandの生成
            // マスターIDとパスワードの両方が入力されていない限りCommandを受け付けない
            this.DnsUpdateCommand = new[]{
                this.MasterId.ObserveHasErrors,
                this.Password.ObserveHasErrors
            }
            .CombineLatestValuesAreAllFalse()
            .ToReactiveCommand();

            this.DnsUpdateCommand.Subscribe(_ =>
            {
                UpdateMydnsServer();
            });
        }


        private void UpdateMydnsServer()
        {
            Console.WriteLine("この変に簡易Model作る");
        }

    }
}
