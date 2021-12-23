using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Exercise2.Commands.Base;
using Exercise2.Models;
using RecognizerModels;


namespace Exercise2.ViewModels
{
    internal class MainWindowViewModel: ViewModelBase
    {
        private readonly Recognizer _recognizer;

        //-------------------------------------------------------------------
        private bool _IsRecognition = true;
        public bool IsRecognition
        {
            get => _IsRecognition;
            set => Set(ref _IsRecognition, value);
        }

        //-------------------------------------------------------------------
        public ObservableCollection<RecognizedImage> RecognizedImage { get; }

        private RecognizedImage _SelectedImage;
        public RecognizedImage SelectedImage
        {
            get => _SelectedImage;
            set => Set(ref _SelectedImage, value);
        }

        //-------------------------------------------------------------------
        public Command StartRecognitionCommand { get; }
        private async void StartRecognitionCommandExecute(object _)
        {
            IsRecognition = false;
            await StartRecognition();
            IsRecognition = true;
        }
        private bool StartRecognitionCommandCanExecute(object _)
        {
            return IsRecognition;
        }

        public Command CancelRecognitionCommand { get; }
        private void CancelRecognitionCommandExecute(object _)
        {
            CancelRecognition();
            IsRecognition = true;
        }
        private bool CancelRecognitionCommandCanExecute(object _)
        {
            return !IsRecognition;
        }

        //-------------------------------------------------------------------
        public MainWindowViewModel()
        {
            _recognizer = new Recognizer();
            RecognizedImage = new ObservableCollection<RecognizedImage>();

            StartRecognitionCommand = new Command(StartRecognitionCommandExecute, StartRecognitionCommandCanExecute);
            CancelRecognitionCommand = new Command(CancelRecognitionCommandExecute, CancelRecognitionCommandCanExecute);
        }

        //-------------------------------------------------------------------
        private async Task StartRecognition()
        {
            var folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();

            if (folderDialog.ShowDialog() ?? false)
            {
                RecognizedImage.Clear();
                var folderPath = folderDialog.SelectedPaths[0];
                var image2id = new Dictionary<string, int>();

                int maxId = 0;
                foreach (var imageFileInfo in new DirectoryInfo(folderPath).GetFiles())
                {
                    RecognizedImage.Add(new RecognizedImage(imageFileInfo.Name, imageFileInfo.FullName));
                    image2id[imageFileInfo.FullName] = maxId;
                    ++maxId;
                }

                await foreach (var (imagePath, predict) in _recognizer.StartRecognition(folderPath))
                {
                    RecognizedImage[image2id[imagePath]].BBoxes.Add(new BBox(predict));
                }

                MessageBox.Show("Распознование окончено.", "Внимание");
            }
        }

        private void CancelRecognition()
        {
            _recognizer.CancelRecognition();
            MessageBox.Show("Вы остановили процесс распознования.", "Внимание");
        }
    }
}
