using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        string data;
        int tr_voltage;
        double degs;
        double speed;
        double signal;
        double time;
        double PWMvoltage;
        double u;
        double error;
        


        StreamWriter fichero;
        bool is_recording = false;





        public Form1()
        {
            InitializeComponent();
            Initports();
            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
            

        }


        //connect_button
        private void button4_Click(object sender, EventArgs e)
        {
            serialPort1.PortName = comboBox1.Text;
            serialPort1.Open();
        }
        //combo_box
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            serialPort1.PortName = comboBox1.Text;
        }

        private void Initports()
        {
            string[] Ports = SerialPort.GetPortNames();
            comboBox1.Items.Clear();
            for (int i = 0; i < Ports.Length; i++)
            {
                comboBox1.Items.Add(Ports[i]);
            }
        }
        //start button
        private void button1_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Write("a\r");
            }
        }

        //stopbutton
        private void button2_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Write("b\r");
            }
        }

        //spin direction
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Write("c\r");
            }
        }
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Write("d\r");
            }
        }

        //GETSerialDATA
        private void GetSerialDataFromArduino(object sender, EventArgs e)
        {

            if (serialPort1.IsOpen)
            {
                data = serialPort1.ReadLine();

                if (data[0] == 'B')
                {
                    richTextBox1.AppendText(data.Substring(1));
                    richTextBox1.ScrollToCaret();
                }
                else
                {
                    try 
                    {
                        string[] values = data.Split(' ');

                        degs = Convert.ToDouble(values[0]) % 360;
                        speed = Convert.ToDouble(values[1]);
                        signal = Convert.ToDouble(values[2]);
                        time = Convert.ToDouble(values[3])/1000;
                        PWMvoltage = Convert.ToDouble(values[4]);
                        u = Convert.ToDouble(values[5]);
                        error = Convert.ToDouble(values[6]);
                      

                        updateChart();

                        if (is_recording)
                        {
                            fichero.WriteLine(degs + "  " + speed + "  "  + PWMvoltage + "  " + signal + "  " + time + "  " + u + "  " + error);
                        }

                    }
                    catch { }
                }

            }

        }

        private void serialPort1_DataReceived_1(object sender, SerialDataReceivedEventArgs e)
        {
            Invoke(new EventHandler(GetSerialDataFromArduino));
        }



        private void updateChart()
        {
            chart1.ChartAreas[0].RecalculateAxesScale();
            chart2.ChartAreas[0].RecalculateAxesScale();
            chart3.ChartAreas[0].RecalculateAxesScale();
            chart4.ChartAreas[0].RecalculateAxesScale();
            
            chart1.Series["Position"].Points.AddXY(time, degs);        
            
            chart2.Series["Speed"].Points.AddXY(time, speed);
            
            chart3.Series["Error"].Points.AddXY(time, error);

            chart4.Series["Voltage"].Points.AddXY(time, u);
            

            if (comboBox2.SelectedItem.ToString().CompareTo("Position") == 0)
            {
                chart1.Series["Reference"].Points.AddXY(time, signal);
                
                chart2.Series["Reference"].Points.AddXY(time, 0);
            }
           else if(comboBox2.SelectedItem.ToString().CompareTo("Speed") == 0)
           {
                //chart2.Series["Reference"].Points.AddXY(time, signal);
                //chart2.Series["Speed"].Points.AddXY(time, speed);
                chart2.Series["Reference"].Points.AddXY(time, signal);

                chart1.Series["Reference"].Points.AddXY(time, 0);
            }

          

           if (time > 4)
           {
                chart2.Series["Speed"].Points.RemoveAt(0);
                chart1.Series["Position"].Points.RemoveAt(0);
                chart3.Series["Error"].Points.RemoveAt(0);
                chart4.Series["Voltage"].Points.RemoveAt(0);
                chart1.Series["Reference"].Points.RemoveAt(0);
                chart2.Series["Reference"].Points.RemoveAt(0);
            }

           /*if (chart1.Series["Reference"].Points.Count > 400)
           {
                chart1.Series["Reference"].Points.RemoveAt(0);
           }

            if (chart2.Series["Reference"].Points.Count > 400)
            {
                chart2.Series["Reference"].Points.RemoveAt(0);
            }*/
        }

        //disable pin
        private void radioButton8_CheckedChanged(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                if (radioButton9.Checked)
                {
                    serialPort1.Write("e\r"); 
                }
                else
                {
                    serialPort1.Write("f\r");
                }
                    
 
            }
        }

        /*private void radioButton9_CheckedChanged(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                
                
            }
        }*/

        //trackbar
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            tr_voltage = trackBar1.Value;
            if (serialPort1.IsOpen)
            {
                serialPort1.Write("n" + tr_voltage + "\r");
            }
        }

        //reference signal variables
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                
                serialPort1.Write("M" + textBox1.Text + "\r");

            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                
                serialPort1.Write("N" + textBox2.Text + "\r");

            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                
                serialPort1.Write("F" + textBox3.Text + "\r");

            }
        }

        //Waveforms RadioButtons
        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Write("s1\r");
                controller();
            }
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Write("s2\r");
                controller();
            }
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Write("s3\r");
                controller();
            }
        }
        //random 
       
        //manual signal
        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            
            if(radioButton6.Checked)  trackBar2.Enabled = true;
            else trackBar2.Enabled = false;

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (comboBox2.Click) radioButton6.Enabled;
            controller();
            updateChart();
        }
       
        private void trackBar2_Scroll(object sender, EventArgs e)
        {

            //send to arduino  trackBar2.Value;
           
           
            if (serialPort1.IsOpen)
            {
                serialPort1.Write("m" + trackBar2.Value + "\r");
            }
        }
        //chart
        private void chart1_Click(object sender, EventArgs e)
        {
            
        }

        private void chart2_Click_1(object sender, EventArgs e)
        {

        }

        private void chart3_Click(object sender, EventArgs e)
        {

        }

        //record Button

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(is_recording == false)
            {
                fichero = new StreamWriter(textBox4.Text + ".txt");
                is_recording = true;
                button3.Text = "Stop recording";
            }
            else
            {
                is_recording = false;
                fichero.Flush();
                fichero.Close();
                button3.Text = "Start recording";

            }
        }

        //controller combobox
        private void comboBox3_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            controller();
        }

        private void controller()
        {
                if (comboBox3.SelectedItem.ToString().CompareTo("PI") == 0)
                {
                    if (comboBox2.SelectedItem.ToString().CompareTo("Position") == 0)
                    {
                        if (serialPort1.IsOpen)
                        {
                            serialPort1.Write("C\r");
                            textBox6.Text = "0.0449";
                            textBox5.Text = "0.00358";
                        }
                    }
                    else if (comboBox2.SelectedItem.ToString().CompareTo("Speed") == 0)
                    {
                        if (serialPort1.IsOpen)
                        {
                            serialPort1.Write("A\r");
                            textBox6.Text = "0.00888";
                            textBox5.Text = "0.124";
                        }
                    }

                }

                if (comboBox3.SelectedItem.ToString().CompareTo("PID") == 0)
                {
                    if (comboBox2.SelectedItem.ToString().CompareTo("Position") == 0)
                    {
                        if (serialPort1.IsOpen)
                        {
                            serialPort1.Write("D\r");
                            textBox6.Text = "0.06467";
                            textBox5.Text = "0.04329";
                            textBox7.Text = "0.003145";
                        }
                    }
                    else if (comboBox2.SelectedItem.ToString().CompareTo("Speed") == 0)
                    {
                        if (serialPort1.IsOpen)
                        {
                            serialPort1.Write("B\r");
                            textBox6.Text = "0.00527";
                            textBox5.Text = "0.0986";
                            textBox7.Text = "0.0000199";
                        }
                    }

                }

            
        }
        //KI KP KD
        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            serialPort1.Write("i" + textBox5.Text + "\r");
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            serialPort1.Write("p" + textBox6.Text + "\r");
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            serialPort1.Write("o" + textBox7.Text + "\r");
        }

        private void groupBox6_Enter(object sender, EventArgs e)
        {

        }
    }
    
}


//chart1.Series["Reference"].Points.AddXY(time, signal);