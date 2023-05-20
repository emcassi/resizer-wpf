using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Windows.Media.Media3D;

namespace Resizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public class ImageFile : INotifyPropertyChanged
    {
        private string _path;
        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                OnPropertyChanged();
            }
        }

        private int _width;
        public int Width
        {
            get { return _width; }
            set
            {
                _width = value;
                OnPropertyChanged();
            }
        }

        private int _height;
        public int Height
        {
            get { return _height; }
            set
            {
                _height = value;
                OnPropertyChanged();
            }
        }

        private int _newWidth;
        public int NewWidth
        {
            get { return _newWidth; }
            set
            {
                _newWidth = value;
                OnPropertyChanged();
            }
        }

        private int _newHeight;
        public int NewHeight
        {
            get { return _newHeight; }
            set
            {
                _newHeight = value;
                OnPropertyChanged();
            }
        }

        public ImageFile()
        {
            // Default constructor
        }

        public ImageFile(string path, int width, int height)
        {
            Path = path;
            Width = width;
            Height = height;
            NewWidth = width;
            NewHeight = height;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public partial class MainWindow : Window
    {

        List<ImageFile> imgs = new List<ImageFile>();

        public MainWindow()
        {
            InitializeComponent();
            lvImgs.ItemsSource = imgs;
            cbLockAxes.IsChecked = true;
            tbSaveDir.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

        }

        private void btnSaveDir_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = tbSaveDir.Text;
            dialog.IsFolderPicker = true;
            
            if(dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var folder = dialog.FileName;
                tbSaveDir.Text = folder;
            }

        }     

        private void TextChanged(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (cbLockAxes != null)
            {
                if (cbLockAxes.IsChecked == true)
                {
                    var newText = ((TextBox)sender).Text;
                    tbX.Text = newText;
                    tbY.Text = newText;
                }

                ReadyForResize();
            }
        }

        private void spFilePicker_DragOver(object sender, DragEventArgs e)
        {
            if(e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                e.Effects = DragDropEffects.All;
                e.Handled = true;
            }
        }

        private void spFilePicker_Drop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, true);
            string alreadyAdded = "";

            foreach (string file in files)
            {
                if (System.IO.Path.GetExtension(file).ToLower() == ".jpg" ||
                    System.IO.Path.GetExtension(file).ToLower() == ".png" ||
                    System.IO.Path.GetExtension(file).ToLower() == ".bmp" ||
                    System.IO.Path.GetExtension(file).ToLower() == ".gif" ||
                    System.IO.Path.GetExtension(file).ToLower() == ".jpeg")
                {
                    if (!AddImage(file))
                    {
                        alreadyAdded += file + "\n\n";
                    }
                }
            }

            if(alreadyAdded.Length > 0)
            {
                MessageBox.Show("The following images were already added: \n" + alreadyAdded);
            }
        }

        private void spFilePicker_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();

            dialog.Multiselect = true;
            dialog.Filters.Add(new CommonFileDialogFilter("Image files", "*.png;*.jpg;*.jpeg;*.gif;*.bmp"));
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string alreadyAdded = "";
                foreach (var file in dialog.FileNames)
                {
                    if (!AddImage(file))
                    {
                        alreadyAdded += file + "\n\n";
                    }
                }
                if(alreadyAdded.Length > 0)
                {
                    MessageBox.Show("The following images were already added: \n" + alreadyAdded);
                }

            }
        }

        private bool AddImage(string path)
        {
            if (imgs.Any(img => img.Path == path))
            {
                return false;
            }


            using (Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>(path))
            {
                int width = image.Width;
                int height = image.Height;

                ImageFile img = new ImageFile(path, width, height);

                imgs.Add(img);
                lvImgs.ItemsSource = null;
                lvImgs.ItemsSource = imgs;
                lvImgs.Items.Refresh();
            }

            ReadyForResize();

            return true;
        }

        private void ReadyForResize()
        {
            foreach(ImageFile img in imgs)
            {
                if(tbX.Text != "")
                    img.NewWidth = (int)((float)img.Width * float.Parse(tbX.Text) / 100);
                if(tbY.Text != "")
                    img.NewHeight = (int)((float)img.Height * float.Parse(tbY.Text) / 100);
            }

        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ImageFile img = (ImageFile)((System.Windows.Controls.Image)sender).DataContext;

            Process.Start(new ProcessStartInfo
            {
                FileName = img.Path,
                UseShellExecute = true
            });
        }

        private void lvImgs_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            if (svMain != null)
            {
                svMain.ScrollToVerticalOffset(svMain.VerticalOffset - e.Delta);
            }
        }

        private void Delete_Item(object sender, RoutedEventArgs e)
        {
            ImageFile img = (ImageFile)((Button)sender).DataContext;
            imgs.Remove(img);
            lvImgs.Items.Refresh();
        }

        private void field_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox textBox = (TextBox)sender;
                textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

                e.Handled = true; // Prevent further handling of the Enter key event
            }
        }

        private void Resize(object sender, RoutedEventArgs e)
        {
            string folder = "";

            // Iterate over the tempList
            foreach (ImageFile img in imgs)
            {
                folder = tbSaveDir.Text;

                // Extract the filename from the image path
                string filename = System.IO.Path.GetFileName(img.Path);


                // Load the image using SixLabors.ImageSharp
                using (var slImage = SixLabors.ImageSharp.Image.Load<Rgba32>(img.Path))
                {
                    // Resize the image
                    slImage.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new SixLabors.ImageSharp.Size(img.NewWidth, img.NewHeight),
                        Mode = SixLabors.ImageSharp.Processing.ResizeMode.Max,
                    }));

                    string savePath = System.IO.Path.Combine(folder, filename);
                    
                    try
                    {
                        slImage.Save(savePath);
                    } catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }

            imgs.Clear();
            lvImgs.Items.Refresh();

            MessageBoxResult result = MessageBox.Show("The images were resized successfully. Would you like to see the images?", "Success", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = tbSaveDir.Text,
                        UseShellExecute = true
                    };

                    Process.Start(startInfo);
                }
                catch (Exception ex)
                {
                    // Handle any exception that might occur when attempting to open the folder
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
            else if (result == MessageBoxResult.No)
            {
                
            }
        }
    }
}
