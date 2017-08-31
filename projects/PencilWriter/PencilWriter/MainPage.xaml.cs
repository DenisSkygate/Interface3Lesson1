using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace PencilWriter
{
    public sealed partial class MainPage : Page
    {
        public String StackText { get; set; }
        InkRecognizerContainer inkRecognizerContainer = null;
        private IReadOnlyList<InkRecognizer> recoView = null;

        public MainPage()
        {
            StackText = "";
            this.InitializeComponent();
            InkDrawingAttributes drawingAttributes = new InkDrawingAttributes();
            drawingAttributes.Size = new Size(3, 3);
            drawingAttributes.IgnorePressure = false;
            drawingAttributes.FitToCurve = true;
            InkCanvas.InkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
            InkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);
            InkCanvas.InkPresenter.InputDeviceTypes =
        Windows.UI.Core.CoreInputDeviceTypes.Mouse |
        Windows.UI.Core.CoreInputDeviceTypes.Pen |
        Windows.UI.Core.CoreInputDeviceTypes.Touch;
            inkRecognizerContainer = new InkRecognizerContainer();
            recoView = inkRecognizerContainer.GetRecognizers();
            if (recoView.Count > 0)
            {
                foreach (InkRecognizer recognizer in recoView)
                    RecoName.Items.Add(recognizer.Name);
            }

            else
            {
                RecoName.IsEnabled = false;
                RecoName.Items.Add("No Recognizer Available");
            }
            RecoName.SelectedIndex = 0;

            Windows.ApplicationModel.DataTransfer.DataTransferManager.GetForCurrentView().DataRequested += MainPage_DataRequested;
        }

        private async void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            TutoStack.Visibility = Visibility.Collapsed;
            IReadOnlyList<InkStroke> currentStrokes =
     InkCanvas.InkPresenter.StrokeContainer.GetStrokes();



            if (currentStrokes.Count > 0)
            {
                RecoName.IsEnabled = false;

                var recognitionResults = await inkRecognizerContainer.RecognizeAsync(
                    InkCanvas.InkPresenter.StrokeContainer,
                    InkRecognitionTarget.All);

                if (recognitionResults.Count > 0)
                {
                    // Display recognition result
                    this.TextBox.Text = "";

                    foreach (var r in recognitionResults)
                    {
                        this.TextBox.Text += " " + r.GetTextCandidates()[0];
                    }
                    TextBox.SelectionStart = TextBox.Text.Length; // add some logic if length is 0
                    TextBox.SelectionLength = 0;
                }
                RecoName.IsEnabled = true;
            }
        }

        void OnClear(object sender, RoutedEventArgs e)
        {
            InkCanvas.InkPresenter.StrokeContainer.Clear();
            TextBox.Text = "";
        }
        private void OnRecognizerChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedValue = (string)RecoName.SelectedValue;
            SetRecognizerByName(selectedValue);
        }
        bool SetRecognizerByName(string recognizerName)
        {
            bool recognizerFound = false;

            foreach (InkRecognizer reco in recoView)
            {
                if (recognizerName == reco.Name)
                {
                    inkRecognizerContainer.SetDefaultRecognizer(reco);
                    recognizerFound = true;
                    break;
                }
            }
            if (!recognizerFound)
            {
                TextBox.Text = "Could not find target recognizer.";
            }
            return recognizerFound;
        }

        private void Share_Click(object sender, RoutedEventArgs e)
        {
            Windows.ApplicationModel.DataTransfer.DataTransferManager.ShowShareUI();
        }
        void MainPage_DataRequested(Windows.ApplicationModel.DataTransfer.DataTransferManager sender, Windows.ApplicationModel.DataTransfer.DataRequestedEventArgs args)
        {
            String txt;
            txt = stackShare ? StackText : TextBox.Text;

            if (!string.IsNullOrEmpty(txt))
            {
                args.Request.Data.SetText(txt);
                args.Request.Data.Properties.Title = Windows.ApplicationModel.Package.Current.DisplayName;
            }
            else
            {
                args.Request.FailWithDisplayText("Nothing to share");
            }
            stackShare = false;
        }

        private void AddToStack_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(this.TextBox.Text))
            {
                this.StackText += this.TextBox.Text + "\n";
                OnClear(null, null);
                this.StackButton.Foreground = new SolidColorBrush(Color.FromArgb(255, 51, 204, 51));
            }
        }
        private bool stackShare;
        private void Stack_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(StackText))
            {
                MessageDialog messageDialog = new MessageDialog(StackText);
                messageDialog.Commands.Add(new UICommand("Share", (command) => { ShareStack(); }));
                messageDialog.Commands.Add(new UICommand("Clear", (command) => { ClearStack(); }));
                messageDialog.Commands.Add(new UICommand("Close", (command) => { }));
                messageDialog.ShowAsync();
                return;
            }
        }

        private void ClearStack()
        {
            StackText = "";
            this.StackButton.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));

        }

        private void ShareStack()
        {
            stackShare = true;
            Windows.ApplicationModel.DataTransfer.DataTransferManager.ShowShareUI();
        }
    }
}
