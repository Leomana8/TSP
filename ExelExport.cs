
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using Excel = Microsoft.Office.Interop.Excel;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

namespace TSP
{
    class ResaultAlgorithm
    {
        public class Resault
        {
            private double _time;
            public double Time
            {
                get { return _time; }
                set { _time = value; }
            }
            private double _length;
            public double Length
            {
                get { return _length; }
                set { _length = value; }
            }

            public Resault(double time, double length)
            {
                _time = time;
                _length = length;
            }
        };
        
        private string _name;
        public string Name
        {
            get { return _name; }
        }

        private int _countCities;
        public int CountCities
        {
            get { return _countCities; }
        }
        private List<Resault> _data;

        public Resault this[int i]
        {
            get { return _data[i]; }
            set { _data[i] = value; }
        }

        public int CountData
        {
            get { return _data.Count; }
        }

        public ResaultAlgorithm (int countCities, string name)
        {
            _countCities = countCities;
            _name = name;
            _data =new List<Resault>();
        }

        public void Add(double time, double length)
        {
            _data.Add (new Resault(time, length));
        }
    }

    class ExelExport
    {
        public void Save( ResaultAlgorithm[] data)
        {
            {
                //создаём новое Excel приложение
                Excel.Application exApp = new Excel.Application();

                //добавляем рабочую книгу
                exApp.Workbooks.Add();

                //обращаемся к активному листу (по умолчанию он первый)
                Worksheet workSheet = (Worksheet)exApp.ActiveSheet;

                //добавляем строку в Excel файл
                workSheet.Cells[1, 1] = "Вершин";
                for (int j = 0; j < data[0].CountData; j ++)
                {
                    workSheet.Cells[j + 2, 1] = data[0].CountCities;
                }
                for (int i = 0; i < data.Length; i++)
                {
                    int c = (i + 1) * 2;
                    workSheet.Cells[1, c] = data[i].Name + " длина";
                    workSheet.Cells[1, c + 1] = data[i].Name + " время";
                    for (int j = 0; j < data[i].CountData; j++)
                    {
                        ResaultAlgorithm.Resault res = data[i][j];
                        workSheet.Cells[j + 2, c] = res.Length;
                        workSheet.Cells[j + 2, c + 1] = res.Time;
                    }
                }
                workSheet.Columns.AutoFit();
                object misValue = System.Reflection.Missing.Value;

                //Сохранение в Excel файл;
                string path = AppDomain.CurrentDomain.BaseDirectory + data[0].CountCities + "_TSP.xlsx";
                try
                {
                    workSheet.SaveAs(path, 51);
                }
                catch (COMException e)
                {
                    MessageBox.Show("Не сохранено. Закройте документ " + path);
                }

                exApp.Quit();
            }

        }
    }
}
