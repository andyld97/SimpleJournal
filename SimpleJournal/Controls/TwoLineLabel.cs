﻿using SimpleJournal.Documents.UI.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace SimpleJournal.Controls
{
    /// <summary>
    /// https://github.com/fluentribbon/Fluent.Ribbon/blob/develop/Fluent.Ribbon/Themes/Controls/TwoLineLabel.xaml
    /// https://github.com/fluentribbon/Fluent.Ribbon/blob/develop/Fluent.Ribbon/Controls/TwoLineLabel.cs
    /// </summary>
    [DefaultProperty(nameof(Text))]
    [ContentProperty(nameof(Text))]
    [TemplatePart(Name = "PART_TextRun", Type = typeof(AccessText))]
    [TemplatePart(Name = "PART_TextRun2", Type = typeof(AccessText))]
    public class TwoLineLabel : Control
    {
        #region Fields

        /// <summary>
        /// Run with text
        /// </summary>
        private AccessText? textRun;

        private AccessText? textRun2;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets whether label must have two lines
        /// </summary>
        public bool HasTwoLines
        {
            get { return (bool)this.GetValue(HasTwoLinesProperty); }
            set { this.SetValue(HasTwoLinesProperty, Convert.ToBoolean(value)); }
        }

        /// <summary>Identifies the <see cref="HasTwoLines"/> dependency property.</summary>
        public static readonly DependencyProperty HasTwoLinesProperty =
            DependencyProperty.Register(nameof(HasTwoLines), typeof(bool), typeof(TwoLineLabel), new PropertyMetadata(true, OnHasTwoLinesChanged));

        /// <summary>
        /// Handles HasTwoLines property changes
        /// </summary>
        /// <param name="d">Object</param>
        /// <param name="e">The event data</param>
        private static void OnHasTwoLinesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TwoLineLabel)d).UpdateTextRun();
        }

        /// <summary>
        /// Gets or sets whether label has glyph
        /// </summary>
        public bool HasGlyph
        {
            get { return (bool)this.GetValue(HasGlyphProperty); }
            set { this.SetValue(HasGlyphProperty, Convert.ToBoolean(value)); }
        }

        /// <summary>Identifies the <see cref="HasGlyph"/> dependency property.</summary>
        public static readonly DependencyProperty HasGlyphProperty =
            DependencyProperty.Register(nameof(HasGlyph), typeof(bool), typeof(TwoLineLabel), new PropertyMetadata(false, OnHasGlyphChanged));

        /// <summary>
        /// Handles HasGlyph property changes
        /// </summary>
        /// <param name="d">Object</param>
        /// <param name="e">The event data</param>
        private static void OnHasGlyphChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TwoLineLabel)d).UpdateTextRun();
        }

        /// <summary>
        /// Gets or sets the text
        /// </summary>
#pragma warning disable WPF0012
        public string? Text
#pragma warning restore WPF0012
        {
            get { return (string?)this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        /// <summary>Identifies the <see cref="Text"/> dependency property.</summary>
        public static readonly DependencyProperty TextProperty =
#pragma warning disable WPF0010 // Default value type must match registered type.
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(TwoLineLabel), new PropertyMetadata(string.Empty, OnTextChanged));
#pragma warning restore WPF0010 // Default value type must match registered type.

        #endregion

        #region Initialize

        /// <summary>
        /// Static constructor
        /// </summary>
        static TwoLineLabel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TwoLineLabel), new FrameworkPropertyMetadata(typeof(TwoLineLabel)));

            FocusableProperty.OverrideMetadata(typeof(TwoLineLabel), new FrameworkPropertyMetadata(false));
        }

        public TwoLineLabel()
        {
            OnApplyTemplate();
        }

        #endregion

        #region Overrides

        /// <inheritdoc />
        public override void OnApplyTemplate()
        {            
            this.textRun = this.GetTemplateChild("PART_TextRun") as AccessText;
            this.textRun2 = this.GetTemplateChild("PART_TextRun2") as AccessText;

            this.UpdateTextRun();
        }

        /// <inheritdoc />
        protected override AutomationPeer OnCreateAutomationPeer() => null; // new Fluent.Automation.Peers.TwoLineLabelAutomationPeer(this);

        #endregion

        #region Event handling

        /// <summary>
        /// Handles text property changes
        /// </summary>
        /// <param name="d">Object</param>
        /// <param name="e">The event data</param>
        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var label = (TwoLineLabel)d;
            label.UpdateTextRun();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Special behavior for the DropDownToggleButton: only use the first line for text (nowrap, character-ellipsis)
        /// </summary>
        private void UpdateTextRun()
        {
            if (this.textRun is null || this.textRun2 is null)
                return;

            var parent = UIHelper.FindParent<DropDownToggleButton>(this);
            if (parent != null && !parent.HasModifiedTwoLineLabel)
            {
                UpdateTextRunOrg();
                return;
            }

            var text = this.Text?.Trim();
            this.textRun.Text = text;
            this.textRun2.Text = string.Empty;
        }

        private void UpdateTextRunOrg()
        {
            if (this.textRun is null || this.textRun2 is null)
                return;

            var text = this.Text?.Trim();

            if (this.HasTwoLines == false || string.IsNullOrEmpty(text))
            {
                this.textRun.Text = text;
                this.textRun2.Text = string.Empty;
                return;
            }

            // Find soft hyphen, break at its position and display a normal hyphen.
#pragma warning disable CA1307 // Specify StringComparison for clarity
            var hyphenIndex = text!.IndexOf((char)173);
#pragma warning restore CA1307 // Specify StringComparison for clarity

            if (hyphenIndex >= 0)
            {
                this.textRun.Text = text.Substring(0, hyphenIndex) + "-";
                this.textRun2.Text = text.Substring(hyphenIndex) + " ";
            }
            else
            {
                var centerIndex = text.Length / 2;

                // Find spaces nearest to center from left and right
                var leftSpaceIndex = text.LastIndexOf(" ", centerIndex, centerIndex, StringComparison.CurrentCulture);
                var rightSpaceIndex = text.IndexOf(" ", centerIndex, StringComparison.CurrentCulture);

                if (leftSpaceIndex == -1
                    && rightSpaceIndex == -1)
                {
                    this.textRun.Text = text;
                    this.textRun2.Text = string.Empty;
                }
                else if (leftSpaceIndex == -1)
                {
                    // Finds only space from right. New line adds on it
                    this.textRun.Text = text.Substring(0, rightSpaceIndex);
                    this.textRun2.Text = text.Substring(rightSpaceIndex) + " ";
                }
                else if (rightSpaceIndex == -1)
                {
                    // Finds only space from left. New line adds on it
                    this.textRun.Text = text.Substring(0, leftSpaceIndex);
                    this.textRun2.Text = text.Substring(leftSpaceIndex) + " ";
                }
                else
                {
                    // Find nearest to center space and add new line on it
                    if (Math.Abs(centerIndex - leftSpaceIndex) < Math.Abs(centerIndex - rightSpaceIndex))
                    {
                        this.textRun.Text = text.Substring(0, leftSpaceIndex);
                        this.textRun2.Text = text.Substring(leftSpaceIndex) + " ";
                    }
                    else
                    {
                        this.textRun.Text = text.Substring(0, rightSpaceIndex);
                        this.textRun2.Text = text.Substring(rightSpaceIndex) + " ";
                    }
                }
            }
        }

        #endregion
    }
}