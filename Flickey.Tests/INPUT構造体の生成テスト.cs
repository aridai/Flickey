using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flickey.Tests
{
    using Flickey.Models;
    using Flickey.Models.PInvokeComponents;

    [TestClass]
    public sealed class INPUT構造体の生成テスト
    {
        private InputInfoQuery query;

        [TestInitialize]
        public void TestInitialize()
        {
            var fileName = "Mapping.json";
            var jsonStr =
                @"[
                    { ""character"": ""単一のキー入力しかないケース"", ""mode"": ""Direct"", ""keys"": [ [ ""N1"" ] ] },
                    { ""character"": ""複数回キーを入力するケース"", ""mode"": ""Japanese"", ""keys"": [ [ ""Y"" ], [ ""A"" ], [ ""J"" ], [ ""I"" ], [ ""R"" ], [ ""U"" ], [ ""S"" ], [ ""I"" ] ] },
                    { ""character"": ""同じキーを連続で入力するケース1"", ""mode"": ""Japanese"", ""keys"": [ [ ""N"" ], [ ""N"" ] ] },
                    { ""character"": ""同じキーを連続で入力するケース2"", ""mode"": ""Japanese"", ""keys"": [ [ ""N"" ], [ ], [ ""N"" ] ] },
                    { ""character"": ""装飾キーを同時入力するケース"", ""mode"": ""Direct"", ""keys"": [ [ ""Control"", ""Shift"", ""Escape"" ] ] },
                    { ""character"": ""複雑なショートカットキー入力をするケース1"", ""mode"": ""Direct"", ""keys"": [ [ ""Control"", ""A"" ], [ ""Control"", ""C"" ], [ ""Control"", ""V"" ], [ ""Control"", ""V"" ] ] },
                    { ""character"": ""複雑なショートカットキー入力をするケース2"", ""mode"": ""Direct"", ""keys"": [ [ ""Control"", ""A"" ], [ ""Control"", ""C"" ], [ ""Control"", ""V"" ], [ ], [ ""Control"", ""V"" ] ] },
                    { ""character"": ""複雑なショートカットキー入力をするケース3"", ""mode"": ""Direct"", ""keys"": [ [ ""Control"", ""A"" ], [ ""Control"", ""C"" ], [ ""Control"", ""V"" ], [ ""Control"" ], [ ""Control"", ""V"" ] ] },
                    { ""character"": ""複雑なショートカットキー入力をするケース4"", ""mode"": ""Direct"", ""keys"": [ [ ""Control"", ""A"" ], [ ""Control"", ""C"" ], [ ""Control"", ""V"" ], [ ""Control"" ], [ ""V"", ""Control"" ] ] }
                ]";

            var reader = new TestFileReader();
            reader.RegisterStringPair(fileName, jsonStr);

            this.query = new InputInfoQuery(reader);
        }

        [TestMethod]
        public void 単一のキー入力しかないケース()
        {
            var expected = this.GenerateExpectedStructures(new[]
            {
                (VirtualKeyCode.N1, KeyboardInputFlag.KEYEVENTF_NONE),
                (VirtualKeyCode.N1, KeyboardInputFlag.KEYEVENTF_KEYUP),
            }).ToList();

            var actual = this.query.GetInputInfo("単一のキー入力しかないケース").Structures?.ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void 複数回キーを入力するケース()
        {
            var expected = this.GenerateExpectedStructures(new[]
            {
                (VirtualKeyCode.Y, KeyboardInputFlag.KEYEVENTF_NONE),
                (VirtualKeyCode.Y, KeyboardInputFlag.KEYEVENTF_KEYUP),
                (VirtualKeyCode.A, KeyboardInputFlag.KEYEVENTF_NONE),
                (VirtualKeyCode.A, KeyboardInputFlag.KEYEVENTF_KEYUP),
                (VirtualKeyCode.J, KeyboardInputFlag.KEYEVENTF_NONE),
                (VirtualKeyCode.J, KeyboardInputFlag.KEYEVENTF_KEYUP),
                (VirtualKeyCode.I, KeyboardInputFlag.KEYEVENTF_NONE),
                (VirtualKeyCode.I, KeyboardInputFlag.KEYEVENTF_KEYUP),
                (VirtualKeyCode.R, KeyboardInputFlag.KEYEVENTF_NONE),
                (VirtualKeyCode.R, KeyboardInputFlag.KEYEVENTF_KEYUP),
                (VirtualKeyCode.U, KeyboardInputFlag.KEYEVENTF_NONE),
                (VirtualKeyCode.U, KeyboardInputFlag.KEYEVENTF_KEYUP),
                (VirtualKeyCode.S, KeyboardInputFlag.KEYEVENTF_NONE),
                (VirtualKeyCode.S, KeyboardInputFlag.KEYEVENTF_KEYUP),
                (VirtualKeyCode.I, KeyboardInputFlag.KEYEVENTF_NONE),
                (VirtualKeyCode.I, KeyboardInputFlag.KEYEVENTF_KEYUP),
            }).ToList();

            var actual = this.query.GetInputInfo("複数回キーを入力するケース").Structures?.ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void 同じキーを連続で入力するケース1()
        {
            var expected = this.GenerateExpectedStructures(new[]
            {
                (VirtualKeyCode.N, KeyboardInputFlag.KEYEVENTF_NONE),
                (VirtualKeyCode.N, KeyboardInputFlag.KEYEVENTF_KEYUP),
            }).ToList();

            var actual = this.query.GetInputInfo("同じキーを連続で入力するケース1").Structures?.ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void 同じキーを連続で入力するケース2()
        {
            var expected = this.GenerateExpectedStructures(new[]
            {
                (VirtualKeyCode.N, KeyboardInputFlag.KEYEVENTF_NONE),
                (VirtualKeyCode.N, KeyboardInputFlag.KEYEVENTF_KEYUP),
                (VirtualKeyCode.N, KeyboardInputFlag.KEYEVENTF_NONE),
                (VirtualKeyCode.N, KeyboardInputFlag.KEYEVENTF_KEYUP),
            }).ToList();

            var actual = this.query.GetInputInfo("同じキーを連続で入力するケース2").Structures?.ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void 装飾キーを同時入力するケース()
        {
            var expected = this.GenerateExpectedStructures(new[]
            {
                (VirtualKeyCode.Control, KeyboardInputFlag.KEYEVENTF_NONE),
                (VirtualKeyCode.Shift, KeyboardInputFlag.KEYEVENTF_NONE),
                (VirtualKeyCode.Escape, KeyboardInputFlag.KEYEVENTF_NONE),
                (VirtualKeyCode.Control, KeyboardInputFlag.KEYEVENTF_KEYUP),
                (VirtualKeyCode.Shift, KeyboardInputFlag.KEYEVENTF_KEYUP),
                (VirtualKeyCode.Escape, KeyboardInputFlag.KEYEVENTF_KEYUP),
            }).ToList();

            var actual = this.query.GetInputInfo("装飾キーを同時入力するケース").Structures?.ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void 複雑なショートカットキー入力をするケース1()
        {
            var expected = this.GenerateExpectedStructures(new[]
            {
                (VirtualKeyCode.Control, KeyboardInputFlag.KEYEVENTF_NONE),
                (VirtualKeyCode.A, KeyboardInputFlag.KEYEVENTF_NONE),
                (VirtualKeyCode.A, KeyboardInputFlag.KEYEVENTF_KEYUP),
                (VirtualKeyCode.C, KeyboardInputFlag.KEYEVENTF_NONE),
                (VirtualKeyCode.C, KeyboardInputFlag.KEYEVENTF_KEYUP),
                (VirtualKeyCode.V, KeyboardInputFlag.KEYEVENTF_NONE),
                (VirtualKeyCode.Control, KeyboardInputFlag.KEYEVENTF_KEYUP),
                (VirtualKeyCode.V, KeyboardInputFlag.KEYEVENTF_KEYUP),
            }).ToList();

            var actual = this.query.GetInputInfo("複雑なショートカットキー入力をするケース1").Structures?.ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void 複雑なショートカットキー入力をするケース2()
        {
            var expected = this.GenerateExpectedStructures(new[]
            {
                (VirtualKeyCode.Control, KeyboardInputFlag.KEYEVENTF_NONE),
                (VirtualKeyCode.A, KeyboardInputFlag.KEYEVENTF_NONE),
                (VirtualKeyCode.A, KeyboardInputFlag.KEYEVENTF_KEYUP),
                (VirtualKeyCode.C, KeyboardInputFlag.KEYEVENTF_NONE),
                (VirtualKeyCode.C, KeyboardInputFlag.KEYEVENTF_KEYUP),
                (VirtualKeyCode.V, KeyboardInputFlag.KEYEVENTF_NONE),
                (VirtualKeyCode.Control, KeyboardInputFlag.KEYEVENTF_KEYUP),
                (VirtualKeyCode.V, KeyboardInputFlag.KEYEVENTF_KEYUP),
                (VirtualKeyCode.Control, KeyboardInputFlag.KEYEVENTF_NONE),
                (VirtualKeyCode.V, KeyboardInputFlag.KEYEVENTF_NONE),
                (VirtualKeyCode.Control, KeyboardInputFlag.KEYEVENTF_KEYUP),
                (VirtualKeyCode.V, KeyboardInputFlag.KEYEVENTF_KEYUP),
            }).ToList();

            var actual = this.query.GetInputInfo("複雑なショートカットキー入力をするケース2").Structures?.ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void 複雑なショートカットキー入力をするケース3()
        {
            var expected = this.GenerateExpectedStructures(new[]
            {
                (VirtualKeyCode.Control, KeyboardInputFlag.KEYEVENTF_NONE),
                (VirtualKeyCode.A, KeyboardInputFlag.KEYEVENTF_NONE),
                (VirtualKeyCode.A, KeyboardInputFlag.KEYEVENTF_KEYUP),
                (VirtualKeyCode.C, KeyboardInputFlag.KEYEVENTF_NONE),
                (VirtualKeyCode.C, KeyboardInputFlag.KEYEVENTF_KEYUP),
                (VirtualKeyCode.V, KeyboardInputFlag.KEYEVENTF_NONE),
                (VirtualKeyCode.V, KeyboardInputFlag.KEYEVENTF_KEYUP),
                (VirtualKeyCode.V, KeyboardInputFlag.KEYEVENTF_NONE),
                (VirtualKeyCode.Control, KeyboardInputFlag.KEYEVENTF_KEYUP),
                (VirtualKeyCode.V, KeyboardInputFlag.KEYEVENTF_KEYUP),
            }).ToList();

            var actual = this.query.GetInputInfo("複雑なショートカットキー入力をするケース3").Structures?.ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void 複雑なショートカットキー入力をするケース4()
        {
            var expected = this.GenerateExpectedStructures(new[]
            {
                (VirtualKeyCode.Control, KeyboardInputFlag.KEYEVENTF_NONE),
                (VirtualKeyCode.A, KeyboardInputFlag.KEYEVENTF_NONE),
                (VirtualKeyCode.A, KeyboardInputFlag.KEYEVENTF_KEYUP),
                (VirtualKeyCode.C, KeyboardInputFlag.KEYEVENTF_NONE),
                (VirtualKeyCode.C, KeyboardInputFlag.KEYEVENTF_KEYUP),
                (VirtualKeyCode.V, KeyboardInputFlag.KEYEVENTF_NONE),
                (VirtualKeyCode.V, KeyboardInputFlag.KEYEVENTF_KEYUP),
                (VirtualKeyCode.V, KeyboardInputFlag.KEYEVENTF_NONE),
                (VirtualKeyCode.V, KeyboardInputFlag.KEYEVENTF_KEYUP),
                (VirtualKeyCode.Control, KeyboardInputFlag.KEYEVENTF_KEYUP),
            }).ToList();

            var actual = this.query.GetInputInfo("複雑なショートカットキー入力をするケース4").Structures?.ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        private IReadOnlyList<INPUT> GenerateExpectedStructures(IEnumerable<(VirtualKeyCode virtualKeyCode, KeyboardInputFlag flags)> values)
        {
            var inputs = values.Select(v => new INPUT {
                type = InputType.INPUT_KEYBOARD,
                ki = new KEYBDINPUT { wVk = (ushort)v.virtualKeyCode, dwFlags = v.flags } })
                .ToList();

            return inputs;
        }

        private void OutputInputsElements(IEnumerable<INPUT> inputs)
        {
            foreach (var input in inputs)
            {
                System.Diagnostics.Debug.WriteLine($"{(VirtualKeyCode)input.ki.wVk}, {input.ki.dwFlags}");
            }
        }
    }
}