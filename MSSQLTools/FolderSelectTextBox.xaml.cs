using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using Microsoft.WindowsAPICodePack.Dialogs;

// this is part of <Microsoft SQL Server Tools>.
// for full licensing details, please go to https://github.com/ch3plusStudio/MSSQLTools/blob/master/LICENSE.md

namespace MSSQLTools
{
    /// <summary>
    /// Interaction logic for FolderSelectTextBox.xaml
    /// </summary>
    public partial class FolderSelectTextBox : UserControl
    {
        public FolderSelectTextBox()
        {
            InitializeComponent();

            // Default
            ButtonText = "Browse";
            IsFolderPicker = true;
            FileName = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            ValidateFileName();
        }

        // Property: Button Text
        public String ButtonText { get { return (String) button.Content; } set { button.Content = value; } }

        // Property: File name shown in Textbox
        public String FileName { get { return textBox.Text; } set { textBox.Text = value; } }

        // Property: Update only after file name in Textbox is validated
        public String ValidatedFileName { get; private set; }

        public bool IsFileNameValidate { get; private set; }

        // Property: File or Folder
        public bool IsFolderPicker { get; set; }
        
        // Validate file name in textbox
        // TODO : Add popup
        private void ValidateFileName()
        {
            if(this.IsFolderPicker)
            {
                if ((System.IO.Directory.Exists(this.FileName)))
                {
                    textBox.Background = Brushes.Green;
                    this.ValidatedFileName = textBox.Text;
                    this.IsFileNameValidate = true;
                } else {
                    textBox.Background = Brushes.Red;
                    this.IsFileNameValidate = false;
                }
            }else
            {
                if (System.IO.File.Exists(this.FileName))
                {
                    textBox.Background = Brushes.Green;
                    this.ValidatedFileName = textBox.Text;
                    this.IsFileNameValidate = true;
                }
                else
                {
                    textBox.Background = Brushes.Red;
                    this.IsFileNameValidate = false;
                }
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ValidateFileName();
        }

        private void TextBox_Changed(object sender, TextChangedEventArgs e)
        {
            ValidateFileName();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = this.IsFolderPicker;
            dialog.Multiselect = false; // Textbox only support one folder
            dialog.RestoreDirectory = true;

            dialog.ShowDialog();

            textBox.Text = dialog.FileName;
            ValidateFileName();
        }
    }
}
