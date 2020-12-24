﻿using Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Checkmeg.WPF.Controls
{
    /// <summary>
    /// Interaction logic for BookmarkCreateControl.xaml
    /// </summary>
    public partial class BookmarkCreateControl : UserControl
    {
        [Browsable(true)]
        [Category("Action")]
        [Description("Invoked when user clicks button")]
        public event EventHandler InputEvent;

        public BookmarkCreateControl()
        {
            InitializeComponent();
        }

        public async Task CreateAsync(bool msg)
        {
            if (txt_selection.Text.Length == 0)
                return;

            string bin = tb_category.Text;
            if (bin.Length == 0)
                bin = Engine.Env.TempBinPath;

            bool success = await Engine.Env.FirebaseController.CreateBookmarkAsync(bin, txt_selection.Text);
            ClipboardManager.ClearClipboard();
            if (success && msg)
            {
                await View.MessageBox.FireAsync("Create Bookmark", "Successfully created bookmark.", new List<string>() { "Ok" });
            }
        }

        public void Cancel()
        {
            txt_selection.Text = "";
            tb_category.Clear();
            ClipboardManager.ClearClipboard();
        }

        public void Initialize(string initPath = "", string initContent = "")
        {
            string selectedText = initContent == "" ? ClipboardManager.GetClipboardText() : initContent;

            tb_category.Text = initPath;
            txt_selection.Text = selectedText;

        }

        private void Tb_category_KeyDown(object sender, KeyEventArgs e)
        {
            InputEvent?.Invoke(sender, e);
            if (e.Key == Key.Tab)
            {
                tb_category.CaretIndex = tb_category.Text.Length;
                e.Handled = true;
            }
        }

        private void Txt_selection_KeyDown(object sender, KeyEventArgs e)
        {
            InputEvent?.Invoke(sender, e);
        }

        private void Txt_selection_TextChanged(object sender, EventArgs e)
        {
            if (txt_selection.Text.Length > Engine.Env.CharacterLimit)
            {
                txt_selection.Document.Text = txt_selection.Text.Substring(0, Engine.Env.CharacterLimit);
                txt_selection.CaretOffset = Engine.Env.CharacterLimit;
            }

            tb_characterLimit.Text = txt_selection.Text.Length + "/" + Engine.Env.CharacterLimit;
        }

        private void Tb_category_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tb_category.Text.Length > Engine.Env.BinPathCharacterLimit)
            {
                tb_category.Text = tb_category.Text.Substring(0, Engine.Env.BinPathCharacterLimit);
                tb_category.CaretIndex = Engine.Env.BinPathCharacterLimit;
            }
        }

        private async void Btn_SaveBookmark_ClickAsync(object sender, RoutedEventArgs e)
        {
            await CreateAsync(true);
        }
    }
}
