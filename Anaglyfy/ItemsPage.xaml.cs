using Anaglyfy.Common;
using Anaglyfy.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace Anaglyfy
{
    /// <summary>
    /// A page that displays a collection of item previews.  In the Split App this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class ItemsPage : Page

    {
        
        
        Windows.Graphics.Imaging.PixelDataProvider pixelData;
        byte[] sourcePixels;
        int w, h;



        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
             
        }

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public ItemsPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            var sampleDataGroups = await SampleDataSource.GetGroupsAsync();
            this.DefaultViewModel["Items"] = sampleDataGroups;
            
            
        }

        /// <summary>
        /// Invoked when an item is clicked.
        /// </summary>
        /// <param name="sender">The GridView (or ListView when the application is snapped)
        /// displaying the item clicked.</param>
        /// <param name="e">Event data that describes the item clicked.</param>
        void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Navigate to the appropriate destination page, configuring the new page
            // by passing required information as a navigation parameter
            var groupId = ((SampleDataGroup)e.ClickedItem).UniqueId;
            this.Frame.Navigate(typeof(SplitPage), groupId);
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async   System.Threading.Tasks.Task  open_Click(object sender, RoutedEventArgs e)
        {
            Windows.Graphics.Imaging.BitmapDecoder decoder;
            Guid decoderId;

            Windows.Storage.Streams.IRandomAccessStream fileStream; // Wczytanie pliku do strumienia



            //przyciskiEnabled();
            Windows.Storage.Pickers.FileOpenPicker FOP = new Windows.Storage.Pickers.FileOpenPicker(); // Klasa okna wybierania pliku
            FOP.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail; // Rodzaj podglądu plików w oknie - tu jako małe obrazy
            FOP.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary; // Od jakiego katalogu okno powinno zacząć wyświetlanie
            FOP.FileTypeFilter.Add(".bmp"); // filtry, które informują jakie rozszerzenia można wybrać
            FOP.FileTypeFilter.Add(".jpg");
            FOP.FileTypeFilter.Add(".jpeg");
            FOP.FileTypeFilter.Add(".png");
            FOP.FileTypeFilter.Add(".gif");
            Windows.Storage.StorageFile file = await FOP.PickSingleFileAsync();
            // Uruchomienie wybierania pliku pojedynczego

            if (file != null)
            {
                //przyciskiVisible();

                fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                // Dekoder będzie potrzebny później przy pracy na obrazie
                Windows.UI.Xaml.Media.Imaging.BitmapImage bitmapImage = new Windows.UI.Xaml.Media.Imaging.BitmapImage(); // Stworzenie obiektu obrazu do wyświetlenia
                bitmapImage.SetSource(fileStream); // Przepisanie obrazu ze strumienia do obiektu obrazu przez wartosc
                //this.show.Source = bitmapImage; // Przypisanie obiektu obrazu do elementu interfejsu typu "Image" o nazwie "Oryginał"
                // Poniżej znajduje się zapamiętanie dekodera
                w = bitmapImage.PixelWidth;
                h = bitmapImage.PixelHeight;

                switch (file.FileType.ToLower())
                {
                    case ".jpg":
                    case ".jpeg":
                        decoderId = Windows.Graphics.Imaging.BitmapDecoder.JpegDecoderId;
                        break;
                    case ".bmp":
                        decoderId = Windows.Graphics.Imaging.BitmapDecoder.BmpDecoderId;
                        break;
                    case ".png":
                        decoderId = Windows.Graphics.Imaging.BitmapDecoder.PngDecoderId;
                        break;
                    case ".gif":
                        decoderId = Windows.Graphics.Imaging.BitmapDecoder.GifDecoderId;
                        break;
                    default:
                        return;
                }

                decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(decoderId, fileStream); // Dekodowanie strumienia za pomocą dekodera
                // Dekodowanie strumienia do klasy z informacjami o jego parametrach
                pixelData = await decoder.GetPixelDataAsync(
                Windows.Graphics.Imaging.BitmapPixelFormat.Bgra8,// Warto tu zwrócić uwagę jak przechowywane są kolory!!!
                Windows.Graphics.Imaging.BitmapAlphaMode.Straight,
                new Windows.Graphics.Imaging.BitmapTransform(),
                Windows.Graphics.Imaging.ExifOrientationMode.IgnoreExifOrientation,
                Windows.Graphics.Imaging.ColorManagementMode.DoNotColorManage
                );
                

                //v = new lab01biometria.imageoperation.Otsu();
                //v.rob(ImageToWork);
                //bitmpe(ImageToWork);

            }
               
        }
        private async void open_ClickLeft(object sender, RoutedEventArgs e)
        {
            await open_Click(sender, e);
            sourcePixels = pixelData.DetachPixelData();
            App.ImageLeft = new lab01biometria.image_RGB(sourcePixels, w, h);
            
        }
        private async void open_ClickRight(object sender, RoutedEventArgs e)
        {
            await open_Click(sender, e);
            sourcePixels = pixelData.DetachPixelData();
            App.ImageRight = new lab01biometria.image_RGB(sourcePixels, w, h);

        }
        
    }
}
