using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Reflection;

namespace EmergencyStrobesPatternsEditor
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        private readonly Assembly assembly = Assembly.GetEntryAssembly();

        public string TitleDisplay
        {
            get
            {
                return "About " + ProductName;
            }
        }

        public string Company
        {
            get
            {
                object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);

                if (attributes.Length <= 0)
                    return "";

                AssemblyCompanyAttribute companyAttribute = attributes[0] as AssemblyCompanyAttribute;
                return companyAttribute.Company;
            }
        }

        public string ProductName
        {
            get
            {
                return assembly.GetName().Name;
            }
        }

        public string ProductDescription
        {
            get
            {
                object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);

                if (attributes.Length <= 0)
                    return "";

                AssemblyDescriptionAttribute descAttribute = attributes[0] as AssemblyDescriptionAttribute;
                return descAttribute.Description;
            }
        }

        public string VersionDisplay
        {
            get
            {
                Version version = assembly.GetName().Version;

                if (version == null)
                    return "";
                
                return "Version " + version.ToString();
            }
        }

        public string CopyrightDisplay
        {
            get
            {
                object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);

                if (attributes.Length <= 0)
                    return "";

                AssemblyCopyrightAttribute copyrightAttribute = attributes[0] as AssemblyCopyrightAttribute;
                return copyrightAttribute.Copyright;
            }
        }

        private BitmapImage imageSource;
        public ImageSource ImageSource
        {
            get
            {
                return imageSource;
            }
        }

        public AboutWindow()
        {
            Console.WriteLine(Assembly.GetExecutingAssembly().GetManifestResourceNames().Count());
            foreach (var item in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                Console.WriteLine(item);
            }

            imageSource = new BitmapImage();

            System.IO.Stream iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("EmergencyStrobesPatternsEditor.icon.ico");
            imageSource.BeginInit();
            imageSource.StreamSource = iconStream;
            imageSource.EndInit();
            iconStream.Dispose();

            InitializeComponent();
        }
    }
}
