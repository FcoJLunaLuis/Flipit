using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Flipit.Dialogue.Tests
{
    /// <summary>
    /// Tests for DialogueOptionButton component and Dialogue_UI choice navigation.
    /// Validates wrapping highlight via modulo arithmetic (Property 10).
    /// </summary>
    [TestFixture]
    public class NavigationWrappingTests
    {
        private Dialogue_UI _dialogueUI;
        private DialogueOptionButton[] _optionButtons;
        private GameObject _rootGO;

        [SetUp]
        public void SetUp()
        {
            _rootGO = new GameObject("DialogueUI_Test");

            // Create option buttons with required components
            _optionButtons = new DialogueOptionButton[4];
            for (int i = 0; i < 4; i++)
            {
                var buttonGO = new GameObject($"OptionButton_{i}");
                buttonGO.transform.SetParent(_rootGO.transform);

                // Add TMP_Text child for label
                var labelGO = new GameObject("Label");
                labelGO.transform.SetParent(buttonGO.transform);
                var tmpText = labelGO.AddComponent<TextMeshProUGUI>();

                // Add Image for highlight
                var highlightGO = new GameObject("Highlight");
                highlightGO.transform.SetParent(buttonGO.transform);
                var highlightImage = highlightGO.AddComponent<Image>();

                var optionButton = buttonGO.AddComponent<DialogueOptionButton>();

                // Use reflection to set serialized fields since they are private
                var labelField = typeof(DialogueOptionButton).GetField("labelText",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                labelField.SetValue(optionButton, tmpText);

                var highlightField = typeof(DialogueOptionButton).GetField("highlightImage",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                highlightField.SetValue(optionButton, highlightImage);

                _optionButtons[i] = optionButton;
            }

            // Create Dialogue_UI
            _dialogueUI = _rootGO.AddComponent<Dialogue_UI>();

            // Assign option buttons via reflection
            var optionButtonsField = typeof(Dialogue_UI).GetField("optionButtons",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            optionButtonsField.SetValue(_dialogueUI, _optionButtons);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_rootGO);
        }

        #region DialogueOptionButton Tests

        [Test]
        public void Setup_SetsLabelTextAndHighlight()
        {
            _optionButtons[0].Setup("Hello", true);

            var labelField = typeof(DialogueOptionButton).GetField("labelText",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var label = (TMP_Text)labelField.GetValue(_optionButtons[0]);

            var highlightField = typeof(DialogueOptionButton).GetField("highlightImage",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var highlight = (Image)highlightField.GetValue(_optionButtons[0]);

            Assert.AreEqual("Hello", label.text);
            Assert.IsTrue(highlight.enabled);
        }

        [Test]
        public void Setup_NotHighlighted_DisablesHighlightImage()
        {
            _optionButtons[0].Setup("Option A", false);

            var highlightField = typeof(DialogueOptionButton).GetField("highlightImage",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var highlight = (Image)highlightField.GetValue(_optionButtons[0]);

            Assert.IsFalse(highlight.enabled);
        }

        [Test]
        public void SetHighlighted_True_EnablesHighlightImage()
        {
            _optionButtons[0].Setup("Test", false);
            _optionButtons[0].SetHighlighted(true);

            var highlightField = typeof(DialogueOptionButton).GetField("highlightImage",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var highlight = (Image)highlightField.GetValue(_optionButtons[0]);

            Assert.IsTrue(highlight.enabled);
        }

        [Test]
        public void SetHighlighted_False_DisablesHighlightImage()
        {
            _optionButtons[0].Setup("Test", true);
            _optionButtons[0].SetHighlighted(false);

            var highlightField = typeof(DialogueOptionButton).GetField("highlightImage",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var highlight = (Image)highlightField.GetValue(_optionButtons[0]);

            Assert.IsFalse(highlight.enabled);
        }

        #endregion

        #region Choice Navigation Tests

        [Test]
        public void ShowOptions_FirstOptionHighlightedByDefault()
        {
            var options = CreateOptions(3);
            _dialogueUI.ShowOptions(options, _ => { });

            Assert.AreEqual(0, _dialogueUI.HighlightedIndex);
            Assert.AreEqual(3, _dialogueUI.ActiveOptionCount);
        }

        [Test]
        public void NavigateDown_MovesToNextOption()
        {
            var options = CreateOptions(3);
            _dialogueUI.ShowOptions(options, _ => { });

            _dialogueUI.NavigateDown();

            Assert.AreEqual(1, _dialogueUI.HighlightedIndex);
        }

        [Test]
        public void NavigateDown_WrapsFromLastToFirst()
        {
            var options = CreateOptions(3);
            _dialogueUI.ShowOptions(options, _ => { });

            _dialogueUI.NavigateDown(); // 1
            _dialogueUI.NavigateDown(); // 2
            _dialogueUI.NavigateDown(); // wraps to 0

            Assert.AreEqual(0, _dialogueUI.HighlightedIndex);
        }

        [Test]
        public void NavigateUp_WrapsFromFirstToLast()
        {
            var options = CreateOptions(3);
            _dialogueUI.ShowOptions(options, _ => { });

            _dialogueUI.NavigateUp(); // wraps to 2

            Assert.AreEqual(2, _dialogueUI.HighlightedIndex);
        }

        [Test]
        public void NavigateUp_MovesToPreviousOption()
        {
            var options = CreateOptions(4);
            _dialogueUI.ShowOptions(options, _ => { });

            _dialogueUI.NavigateDown(); // 1
            _dialogueUI.NavigateDown(); // 2
            _dialogueUI.NavigateUp();   // 1

            Assert.AreEqual(1, _dialogueUI.HighlightedIndex);
        }

        [Test]
        public void NavigateDown_SingleOption_StaysAtZero()
        {
            var options = CreateOptions(1);
            _dialogueUI.ShowOptions(options, _ => { });

            _dialogueUI.NavigateDown();

            Assert.AreEqual(0, _dialogueUI.HighlightedIndex);
        }

        [Test]
        public void NavigateUp_SingleOption_StaysAtZero()
        {
            var options = CreateOptions(1);
            _dialogueUI.ShowOptions(options, _ => { });

            _dialogueUI.NavigateUp();

            Assert.AreEqual(0, _dialogueUI.HighlightedIndex);
        }

        [Test]
        public void ShowOptions_LimitsToMaxFourButtons()
        {
            // Even if we pass more options than buttons available, it caps at array length
            var options = CreateOptions(4);
            _dialogueUI.ShowOptions(options, _ => { });

            Assert.AreEqual(4, _dialogueUI.ActiveOptionCount);
        }

        [Test]
        public void NavigateDown_NoActiveOptions_DoesNothing()
        {
            // Don't call ShowOptions - no active options
            _dialogueUI.HideOptions();
            _dialogueUI.NavigateDown();

            Assert.AreEqual(0, _dialogueUI.HighlightedIndex);
        }

        [Test]
        public void NavigateUp_NoActiveOptions_DoesNothing()
        {
            _dialogueUI.HideOptions();
            _dialogueUI.NavigateUp();

            Assert.AreEqual(0, _dialogueUI.HighlightedIndex);
        }

        #endregion

        #region Property-Based Test: Navigation Wrapping (Property 10)

        /// <summary>
        /// Property 10: Choice navigation wrapping
        /// For any set of displayed options (count 1-4) and any sequence of Navigate Up/Down actions,
        /// the highlighted index wraps using modulo arithmetic.
        /// **Validates: Requirements 5.3**
        /// </summary>
        [Test]
        [Category("Property")]
        public void Property10_NavigationWrapping_ModuloArithmetic()
        {
            PropertyTestUtility.ForAll(random =>
            {
                // Generate random option count between 1 and 4
                int optionCount = random.Next(1, 5);
                var options = CreateOptions(optionCount);
                _dialogueUI.ShowOptions(options, _ => { });

                // Start at index 0
                int expectedIndex = 0;
                Assert.AreEqual(expectedIndex, _dialogueUI.HighlightedIndex,
                    $"Initial index should be 0 for {optionCount} options");

                // Perform a random sequence of navigation actions (5-20 steps)
                int steps = random.Next(5, 21);
                for (int s = 0; s < steps; s++)
                {
                    bool navigateDown = random.NextBool();

                    if (navigateDown)
                    {
                        _dialogueUI.NavigateDown();
                        expectedIndex = (expectedIndex + 1) % optionCount;
                    }
                    else
                    {
                        _dialogueUI.NavigateUp();
                        expectedIndex = (expectedIndex - 1 + optionCount) % optionCount;
                    }

                    Assert.AreEqual(expectedIndex, _dialogueUI.HighlightedIndex,
                        $"After step {s} ({(navigateDown ? "Down" : "Up")}), " +
                        $"expected index {expectedIndex} with {optionCount} options");
                }
            });
        }

        #endregion

        #region Helpers

        private List<DialogueOption> CreateOptions(int count)
        {
            var options = new List<DialogueOption>();
            for (int i = 0; i < count; i++)
            {
                options.Add(new DialogueOption($"Option {i + 1}", "", 0));
            }
            return options;
        }

        #endregion
    }
}
