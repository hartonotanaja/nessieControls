using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ServiceModel.Channels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace nessieControls
{
    public sealed partial class NumberBox : TextBox
    {
        public NumberBox()
        {
            this.InitializeComponent();
        }

      
        #region my Props
        public int Digit
        {
            get => (int)GetValue(DigitProperty);
            set => SetValue(DigitProperty, value);
        } 
        private static readonly DependencyProperty DigitProperty =
            DependencyProperty.Register(nameof(Digit), typeof(int), typeof(NumberBox), new PropertyMetadata(0));

        public decimal? Value
        {
            get => (decimal?)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
        private static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(decimal?), typeof(NumberBox), new PropertyMetadata(0));

        public bool ThousandGrouping
        {
            get => (bool)GetValue(ThousandGroupingProperty);
            set => SetValue(ThousandGroupingProperty, value);
        }
        public static readonly DependencyProperty ThousandGroupingProperty =
            DependencyProperty.Register(nameof(ThousandGrouping), typeof(bool), typeof(NumberBox), new PropertyMetadata(false));
        
        public bool AllowZero
        {
            get => (bool)GetValue(AllowZeroProperty);
            set => SetValue(AllowZeroProperty, value);
        }
        private static readonly DependencyProperty AllowZeroProperty =
            DependencyProperty.Register(nameof(AllowZero), typeof(bool), typeof(NumberBox), new PropertyMetadata(true));

        public long MinValue
        {
            get => (long)GetValue(MinValueProperty);
            set => SetValue(MinValueProperty, value);
        }
        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register(nameof(MinValue), typeof(long), typeof(NumberBox), new PropertyMetadata(-1000000000000000));
       

        public long MaxValue
        {
            get => (long)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }
        public readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register(nameof(MaxValue), typeof(long), typeof(NumberBox), new PropertyMetadata(1000000000000000));
       
        #endregion

        #region VirtualKey To Char Translation  coding from external code source
        [DllImport("user32.dll")]
        private static extern int MapVirtualKey(uint uCode, uint uMapType);
        [DllImport("user32.dll")]
        private static extern short GetKeyState(uint nVirtKey);
        private const uint MAPVK_VK_TO_CHAR = 0x02;
        private const uint VK_SHIFT = 0x10;
        private char TranslateVirtualKeyIntoChar(VirtualKey key)
        {
            char c = (char)MapVirtualKey((uint)key, MAPVK_VK_TO_CHAR);
            short shiftState = GetKeyState(VK_SHIFT);
            if (shiftState < 0)
            {
                //shift is pressed
                c = char.ToUpper(c);
            }
            return c;
        }
        #endregion

        private void TextBox_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {

            
            string c = "";
            bool number0 = (e.Key == VirtualKey.Number0) || (e.Key == VirtualKey.NumberPad0);
            bool isNumber = (e.Key >= Windows.System.VirtualKey.Number0) && (e.Key <= Windows.System.VirtualKey.Number9);
            bool isNumPad = (e.Key >= Windows.System.VirtualKey.NumberPad0) && (e.Key <= Windows.System.VirtualKey.NumberPad9);
            bool isDecimal = (e.Key == Windows.System.VirtualKey.Decimal);
            bool isAddition = (e.Key == VirtualKey.Add);
            bool isSubtract = (e.Key == VirtualKey.Subtract);
            char symbol = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];
            char separator = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator[0];
            bool backSpace = (e.Key == VirtualKey.Back);
            bool tab = (e.Key == VirtualKey.Tab);
            bool del = (e.Key == VirtualKey.Delete);
            TextBox tb = sender as TextBox;
            int pos = tb.SelectionStart;
            int selLength = tb.SelectionLength;
            
            if(isNumber)
            {
                c = ((Char)e.Key).ToString();
            }
            else if (isNumPad)
            {
                switch (e.Key)
                {
                    case Windows.System.VirtualKey.NumberPad0:
                        c = "0";
                        break;
                    case Windows.System.VirtualKey.NumberPad1:
                        c = "1";
                        break;
                    case Windows.System.VirtualKey.NumberPad2:
                        c = "2";
                        break;
                    case Windows.System.VirtualKey.NumberPad3:
                        c = "3";
                        break;
                    case Windows.System.VirtualKey.NumberPad4:
                        c = "4";
                        break;
                    case Windows.System.VirtualKey.NumberPad5:
                        c = "5";
                        break;
                    case Windows.System.VirtualKey.NumberPad6:
                        c = "6";
                        break;
                    case Windows.System.VirtualKey.NumberPad7:
                        c = "7";
                        break;
                    case Windows.System.VirtualKey.NumberPad8:
                        c = "8";
                        break;
                    case Windows.System.VirtualKey.NumberPad9:
                        c = "9";
                        break;
                    default:
                        break;
                }
            }
            else
            {
                c = "";
            }

            if (pos<1)
            {
                if (number0 && !AllowZero)
                {
                    e.Handled = true;
                }
                else if (MinValue >= 0 && isSubtract)
                {
                    e.Handled = true;
                }
                else if(isAddition)
                {
                    e.Handled = true;
                }
                else
                {
                    if (isNumber || isNumPad || backSpace || tab || del)
                    {

                    }
                    else
                    {
                        e.Handled = true;
                    }

                }
                
            }
            else if (pos == tb.Text.Length)
            {
                if ((!isAddition && !isSubtract) && isNumber || isNumPad || isDecimal || backSpace || tab || del)
                {

                    if (tb.Text.Substring(0, pos).Contains(symbol))
                    {
                        if (isDecimal) e.Handled = true;
                        //if it is decimal digit then check the allowed digit length
                        else
                        {
                            string[] numbers = tb.Text.Split(symbol);
                            if (string.IsNullOrEmpty(numbers[1]))
                            {

                            }
                            else
                            {
                                if (numbers[1].Length >= Digit && (isNumber || isNumPad))
                                {
                                    e.Handled = true;
                                }
                            }                            
                        }
                         
                    }
                    else
                    {
                        if (isDecimal && Digit>0)
                        {
                            tb.Text += symbol;
                            e.Handled = true;
                            tb.Select(tb.Text.Length, 0);
                        }
                        else
                        {
                            if (!isDecimal && (isNumber || isNumPad))
                            {
                                string newText = tb.Text + TranslateVirtualKeyIntoChar(e.Key);
                                long intValue = long.Parse(newText, NumberStyles.Any, CultureInfo.CurrentCulture);
                                if (intValue > MaxValue)
                                {
                                    e.Handled = true; ;
                                }
                                else
                                {
                                    if (ThousandGrouping)
                                    {
                                        tb.Text = decimal.Parse(intValue.ToString(), NumberStyles.Any).ToString($"N0");
                                        tb.Select(tb.Text.Length, 0);
                                        e.Handled = true;
                                    }
                                    else
                                    {
                                        tb.Text = decimal.Parse(intValue.ToString(), NumberStyles.Any).ToString($"F0");
                                        tb.Select(tb.Text.Length, 0);
                                        e.Handled = true;
                                    }
                                }
                                
                            }
                            else if (backSpace || del || tab)
                            {

                            }
                            else
                            {
                                e.Handled = true;
                               
                            }

                        }
                    }

                }
                else
                {
                    e.Handled = true;
                    
                }
            }
            else
            {
                string text = tb.Text;
                char decSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];
                char thousandSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator[0];
                
                if ((!isAddition && !isSubtract) && isNumber || isNumPad || isDecimal || backSpace || tab || del)
                {
                    if (isDecimal && text.Contains(symbol))
                    {
                        e.Handled = true;
                    }
                    else
                    {
                        text = text.Insert(pos, c);
                  
                        var decNumber = Decimal.Parse(string.Join(null, text.Split(thousandSeparator)), NumberStyles.Any);
                        if (ThousandGrouping)
                        {
                            tb.Text = decNumber.ToString($"N{Digit.ToString()}");
                            tb.Select(tb.Text.Length, 0);
                            e.Handled = true;
                            
                        }
                        else
                        {
                            tb.Text = decNumber.ToString($"F{Digit.ToString()}");
                            tb.Select(tb.Text.Length, 0);
                            e.Handled = true;
                        }
                    }


                }
                else
                {
                    e.Handled = true;
                }


            }

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            char decSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];
            char thousandSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator[0];

            var o = (sender as TextBox).Text;
            if (!string.IsNullOrEmpty(o))
            {
                var number = string.Join(null, o.Split(thousandSeparator));
                Value = Convert.ToDecimal(number);
            }
        }

        public decimal GetValue()
        {
            if (!string.IsNullOrEmpty(tbText.Text))
            {
                return Decimal.Parse(tbText.Text, NumberStyles.Any);
            }
            else
            {
                return 0;
            }
        }
    }
}
