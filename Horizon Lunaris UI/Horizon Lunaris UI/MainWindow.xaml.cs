using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using Horizon_Lunaris_UI.Views;

namespace Horizon_Lunaris_UI

{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Optional: Automatically load a screen when the app first opens
            // MainContentFrame.Content = new DashboardView(); 
        }

        // The routing method for your User Roles button
        private void BtnUserRoles_Click(object sender, RoutedEventArgs e)
        {
            // Swaps the right-hand frame to the new UserRolesView you created
            MainContentFrame.Content = new UserRolesView();
        }
    }
}