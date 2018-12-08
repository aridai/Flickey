using System;
using System.Linq;

namespace Flickey.Models
{
    /// <summary>
    /// 入力処理を管理します。
    /// </summary>
    public sealed class Inputter
    {
        private readonly InputInfoQuery inputInfoQuery;

        private readonly ConversionQuery conversionQuery;

        //  前回の入力文字を保持しておく。
        //  変換ボタンが押されたときに必要になる。
        private string prevCharacter;

        /// <summary>
        /// インスタンスを生成して初期化します。
        /// </summary>
        public Inputter()
        {
            var reader = new FileReader();
            this.inputInfoQuery = new InputInfoQuery(reader);
            this.conversionQuery = new ConversionQuery(reader);
        }

        /// <summary>
        /// 入力処理を行います。
        /// </summary>
        /// <param name="character">入力文字。</param>
        public void Input(string character)
        {
            //  変換キーが押されたなら、前回の入力を変換して再入力する。
            if (this.IsConversionSignal(character))
            {
                this.prevCharacter =
                    this.ConvertPrevCharacter(this.prevCharacter);
            }

            else
            {
                //  マッピングデータが存在するならば、それを使う。
                if (this.inputInfoQuery.GetInputInfo(character) is InputInfo info && info.Structures.Any())
                {
                    this.prevCharacter =
                        this.SendInputWithInputInfo(info);
                }

                //  マッピングデータが存在しないならば、そのまま直接入力する。
                else
                {
                    this.prevCharacter =
                        this.SendInputDirectly(character);
                }
            }
        }

        //  文字が変換キーの入力なのかどうかを判定する。
        private bool IsConversionSignal(string character)
        {
            return (character == "a/A" || character == "゛゜小");
        }

        //  前回の入力を変換して再入力する。
        private string ConvertPrevCharacter(string prevCharacter)
        {
            //  変換後の文字を取得して、存在するならば。
            if (this.conversionQuery.GetConvertedCharacter(prevCharacter) is string converted)
            {
                KeyboardOperator.InputKey(VirtualKeyCode.Back);

                //  変換後の文字も、入力情報が存在すれば、それに従って入力する。
                if (this.inputInfoQuery.GetInputInfo(converted) is InputInfo info && info.Structures.Any())
                    this.SendInputWithInputInfo(info);

                else this.SendInputDirectly(converted);

                return converted;
            }
            
            return prevCharacter;
        }

        //  InputInfoに従って入力を送信する。
        private string SendInputWithInputInfo(InputInfo info)
        {
            //  IMEの入力モードを設定する。
            //  InputMode.Noneの場合は、入力モードを変更せずに、現在の入力モードのままにしておく。
            if (info.InputMode != InputMode.None)
                KeyboardOperator.SetInputMode(info.InputMode);

            //  INPUT構造体のリストをそのまま送信する。
            KeyboardOperator.SendInput(info.Structures);

            return info.Character;
        }

        //  文字をそのまま (キーストロークをシミュレートする方式ではない方式で) 入力する。
        private string SendInputDirectly(string character)
        {
            KeyboardOperator.SetInputMode(InputMode.Direct);
            KeyboardOperator.InputDirectly(character);

            return character;
        }
    }
}