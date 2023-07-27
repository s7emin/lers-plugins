using Lers.Core;
using Lers.Plugins;
using System;
using System.Net.Http;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lers.Utils;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Diagnostics;
using System.Web;

namespace ArshinSearch
{

	

	/// <summary>
	/// Класс экранной формы
	/// </summary>
	public partial class MainForm : Form
	{
		HttpClient client = new HttpClient();
		const string baserequest = "https://fgis.gost.ru/fundmetrology/cm/xcdb/vri/select?";
		JArray docsArray;
		string startpos = "0";
		/// <summary>
		/// Экземпляр хост-интерфейса клиента
		/// </summary>
		IPluginHost host;

		public MainForm()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Инициализация формы
		/// </summary>
		/// <param name="host"></param>
		internal void Initialize(IPluginHost host)
		{
			// Сохраняем хост-интерфейса клиента себе в программу
			this.host = host;
			// Ставим авто-ширину столбцов в такой режим, что суммарная ширина всех столбцов 
			// в точности заполняет отображаемую область MainView
			//MainView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
			/*
			// Получам список точек учёта от сервера
			MeasurePoint[] measurePointList = host.Server.MeasurePoints.GetList();

			// Если список точек учёта не пуст
			if (measurePointList != null)
			{
				// Проходим по всему списку
				foreach (MeasurePoint measurePoint in measurePointList)
				{					
					// Добавляем запись в таболицу MainView
					MainView.Rows.Add(measurePoint.FullTitle, EnumUtils.GetDescription(measurePoint.SystemType));
				}

				
				
			}

			else
				// Если список точек учёта пуст, выводим сообщение
				MessageBox.Show("Список точек учёта пуст",
					"Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			*/
		}

        private void MainView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
			if (e.ColumnIndex == 10) Process.Start("https://fgis.gost.ru/fundmetrology/cm/results/" + MainView.SelectedCells[0].Value.ToString());

		}

        private async void button1_Click(object sender, EventArgs e)
        {
			if (MainView.RowCount > 0)
			{
				int i = 0;
				foreach (var doc in (JArray)docsArray)
				{
					string vri_id = (string)doc.SelectToken("$.['vri_id']");
					string responseBody = await client.GetStringAsync("https://fgis.gost.ru/fundmetrology/cm/iaux/vri/" + vri_id);
					var jsonResponse = JObject.Parse(responseBody);
					string miOwner = (string)jsonResponse.SelectToken("result.vriInfo.miOwner");
					MainView.Rows[i].Cells[11].Value = (miOwner);
					System.Threading.Thread.Sleep(400);
					i++;
					// Добавляем запись в таболицу MainView


				}
			}
			
			


		}

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private async void button2_Click(object sender, EventArgs e)
        {
			button2.Enabled = false;
			MainView.Rows.Clear();
			textBox2.Text = "";
			button1.Enabled = false;
			string requestend = "q=*&fl=vri_id,org_title,mi.mitnumber,mi.mititle,mi.mitype,mi.modification,mi.number,verification_date,valid_date,applicability,result_docnum&sort=verification_date+desc,org_title+asc&rows=100&start=" + startpos;
			string extendrequestnum = "fq=mi.number:" + textBox1.Text + "&";
			string extendrequestorg = "fq=org_title:*" + comboBox1.Text + "*&";
			string extendrequesttype = "fq=mi.mitype:*" + comboBox2.Text + "*&";
			if (!String.IsNullOrEmpty(textBox1.Text)) requestend = extendrequestnum + requestend;
			if (!String.IsNullOrEmpty(comboBox1.Text)) requestend = extendrequestorg + requestend;
			if (!String.IsNullOrEmpty(comboBox2.Text)) requestend = extendrequesttype + requestend;
			
			try {
				string responseBody = await client.GetStringAsync(baserequest + requestend);
				var jsonResponse = JObject.Parse(responseBody);
				docsArray = (JArray)jsonResponse["response"]["docs"];
				textBox2.Text = (string)jsonResponse.SelectToken("response.numFound");
				foreach (var doc in (JArray)docsArray)
				{
					string miMitnumber = (string)doc.SelectToken("$.['mi.mitnumber']");
					string miMititle = (string)doc.SelectToken("$.['mi.mititle']");
					string miMitype = (string)doc.SelectToken("$.['mi.mitype']");
					string miModification = (string)doc.SelectToken("$.['mi.modification']");
					string miNumber = (string)doc.SelectToken("$.['mi.number']");
					string org_title = (string)doc.SelectToken("$.['org_title']");
					string miDate = (string)doc.SelectToken("$.['verification_date']");
					bool applicability = (bool)doc.SelectToken("$.['applicability']");
					string validDate = "-";
					if (applicability) 
					{ 
						validDate = (string)doc.SelectToken("$.['valid_date']");
						validDate = validDate.Substring(0, 10);
					}		
					string vri_id = (string)doc.SelectToken("$.['vri_id']");			
					string miDocnum = (string)doc.SelectToken("$.['result_docnum']");
					//MainView.Rows.Add(miModification, miNumber, org_title, miDate.Substring(0, 10), applicability, vri_id, "-");
					MainView.Rows.Add(org_title, miMitnumber, miMititle, miMitype, miModification, miNumber, miDate.Substring(0, 10), validDate, applicability,miDocnum, vri_id, "-" );
					// Добавляем запись в таболицу MainView
				}
			}
			catch (Exception ex) {
				MessageBox.Show(ex.Message);
					}
			button2.Enabled = true;
			if (MainView.RowCount > 0) button1.Enabled = true;




		}

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
