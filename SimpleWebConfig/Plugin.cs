using Lers.Core;
using Lers.Plugins;
using Lers.UI;
using System.Diagnostics;
using System.Windows.Forms;

namespace SimpleWebConfig
{
    /// <summary>
    /// Класс, реализующий интерфейс IPlugin 
    /// </summary>
    public class Plugin : IPlugin
	{
		/// <summary>
		/// Экземпляр хост-интерфейса клиента
		/// </summary>
		internal static IPluginHost Host { get; private set; }

		/// <summary>
		/// Метод, вызывающийся во время запуска клиента
		/// </summary>
		/// <param name="pluginHost"></param>
		public void Initialize(IPluginHost pluginHost)
		{
			// Копируем экземпляр хост-интерфейса клиента в нашу программу
			Host = pluginHost;
			Plugin.Host.MainWindow.RegisterObjectAction(ObjectType.MeasurePoint, "Открыть web интерфейс", null, OnItemClick);

		}

		/// <summary>
		/// Событие выбора приложения
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnItemClick(int actionId, object sender)
		{
			MeasurePoint measurePoint = (MeasurePoint)sender;

			// Запрашиваем требуемую для работы информацию
			measurePoint.Refresh(MeasurePointInfoFlags.Equipment);

			// Проверим, что у точки учёта есть IP подключение
			bool connectionFound = false;

			foreach (var connection in measurePoint.Device.PollSettings.Connections)
			{
				if (connection.CommLinkType == Lers.Poll.CommunicationLink.Ip)
				{
					try
					{
						Process.Start("http://admin:admin@" + connection.InternetHost);
						connectionFound = true;
						break; // Выходим из цикла, так как нужное подключение найдено
					}
					catch
					{
						MessageBox.Show("Не удалось открыть web интерфейс");
					}
				}
			}

			if (!connectionFound)
			{
				MessageBox.Show(
					"У выбранной точки учёта не задано подключение с типом интернет",
					"Ошибка",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning
				);
			}


		}


	}
}
