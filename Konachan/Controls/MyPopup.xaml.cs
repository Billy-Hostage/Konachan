using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Konachan.Controls
{
    public sealed partial class MyPopup : UserControl
    {
        public MyPopup()
        {
            this.InitializeComponent();
        }

        public async Task Show(string message, int delayInMs = 1500)
        {
            txt_pop.Text = message;
            pop.IsOpen = true;
            await Task.Delay(delayInMs);
            pop.IsOpen = false;
        }
    }
}
