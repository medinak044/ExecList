using ExecList.Models;
using ExecList.Services;

namespace ExecList
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        private readonly IExecListServices _execListServices;

        /// <summary>
        /// Starting point
        /// </summary>
        public MainPage(IExecListServices execListServices)
        {
            InitializeComponent();
            _execListServices = execListServices;

            GenerateButtons();
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }

        private void Refresh(object sender, EventArgs e)
        {
            //TODO find a way to refresh the list of buttons
            GenerateButtons();
        }

        private async void GenerateButtons()
        {
            Profile profile = _execListServices.GetInitialProfile();

            if (profile == null) 
            { 
                await DisplayAlert("Warning", "No profile detected to load!", "OK");
                return;
            }

            foreach (FileItem fileItem in profile.FileItems!)
            {
                var button = new Button
                {
                    Text = !string.IsNullOrEmpty(fileItem.Name) ? 
                    fileItem.Name : Path.GetFileName(fileItem.FilePath),
                    FontSize = 18,
                    HorizontalOptions = LayoutOptions.Fill
                };

                button.Clicked += (sender, e) => Button_Clicked(sender!, e, fileItem);

                ButtonContainer1.Children.Add(button);
            }
        }

        private void Button_Clicked(object sender, EventArgs e, FileItem fileItem)
        {
            _execListServices.OpenFile(fileItem);
        }
    }

}
