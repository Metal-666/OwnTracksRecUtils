using CommunityToolkit.Maui.Storage;

using CsvHelper;
using CsvHelper.Configuration.Attributes;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OwnTracksRecUtils;

public partial class MainPage : ContentPage {

	private static readonly string[] RecExtensions = new[] { ".rec" };

	public MainPage() {

		InitializeComponent();

		Status.Text = "Idle...";
		StatusBackground.BackgroundColor = Colors.DimGray;

	}

	public virtual async void OnMergeRecFiles(object sender, EventArgs args) {

		Status.Text = "Picking files...";
		StatusBackground.BackgroundColor = Colors.DimGray;

		IEnumerable<FileResult> recFiles = await FilePicker.PickMultipleAsync(new() {

			FileTypes = new(new Dictionary<DevicePlatform, IEnumerable<string>>() {

				{ DevicePlatform.WinUI, RecExtensions }

			}),
			PickerTitle = "Please select the .rec files:"

		});

		Status.Text = "Merging files...";

		MemoryStream outputStream = new();

		Stream[] fileStreams =
			await Task.WhenAll(recFiles.Select(file =>
																file.OpenReadAsync()));

		foreach(Stream fileStream in fileStreams) {

			await fileStream.CopyToAsync(outputStream);

		}

		Status.Text = "Saving result...";

		await FileSaver.SaveAsync("merged.rec", outputStream);

		Status.Text = "File merge done!";

	}

	public virtual async void OnRecToCsv(object sender, EventArgs args) {

		Status.Text = "Picking file...";

		FileResult? recFile = await FilePicker.PickAsync(new() {

			FileTypes = new(new Dictionary<DevicePlatform, IEnumerable<string>>() {

				{ DevicePlatform.WinUI, RecExtensions }

			}),
			PickerTitle = "Please select the .rec file:"

		});

		if(recFile == null) {

			Status.Text = "Conversion aborted by user!";
			StatusBackground.BackgroundColor = Colors.OrangeRed;

			return;

		}

		Status.Text = "Parsing file...";

		Stream recStream = await recFile.OpenReadAsync();

		using StreamReader fileReader = new(recStream);

		using MemoryStream memoryStream = new();

		using StreamWriter fileWriter = new(memoryStream);

		using CsvWriter csvWriter =
			new(fileWriter,
						CultureInfo.CurrentCulture);

		csvWriter.WriteHeader<Location>();
		await csvWriter.NextRecordAsync();

		Dictionary<int, string> failedLines = new();

		string? line;
		int lineNumber = 0;

		while((line = await fileReader.ReadLineAsync()) != null) {

			lineNumber++;

			Status.Text = $"Processing line {lineNumber}...";

			Match match = RecLineRegex().Match(line);

			if(!match.Success) {

				failedLines.Add(lineNumber, "Unexpected line formatting");

				continue;

			}

			string json =
				match.Groups
						.Values
						.Last()
						.Value;

			JsonNode? jsonNode = JsonNode.Parse(json);

			JsonNode? type = jsonNode?["_type"];

			if(type == null ||
				type.GetValueKind() != JsonValueKind.String ||
				type.GetValue<string>() != "location") {

				failedLines.Add(lineNumber, $"Unsupported type ({type})");

				continue;

			}

			Location? location =
				jsonNode.Deserialize<Location>();

			if(location == null) {

				failedLines.Add(lineNumber, "JSON parsing failed");

				continue;

			}

			csvWriter.WriteRecord(location);
			await csvWriter.NextRecordAsync();

		}

		await csvWriter.FlushAsync();

		await FileSaver.SaveAsync("result.csv", memoryStream);

		if(failedLines.Count == 0) {

			Status.Text = "Conversion done!";

		}

		else {

			Status.Text =
				"Conversion completed with errors. The following lines could not be converted:\n";

			foreach((int LineNumber, string Reason) in failedLines) {

				Status.Text += $"{LineNumber}: {Reason}\n";

			}

			StatusBackground.BackgroundColor = Colors.OrangeRed;

		}

	}

	[GeneratedRegex(@"^[0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-9]{2}(\.[0-9]+)?([Zz]|([\+-])([01]\d|2[0-3]):?([0-5]\d)?)?\s+\*\s+(\{.*\})$")]
	public static partial Regex RecLineRegex();

}

public class Location {

	public virtual int? Id { get; set; }

	public virtual string? BSSID { get; set; }

	public virtual string? SSID { get; set; }

	[JsonPropertyName("acc")]
	public virtual int? Accuracy { get; set; }

	[JsonPropertyName("alt")]
	public virtual int? Altitude { get; set; }

	[JsonPropertyName("batt")]
	public virtual int? Battery { get; set; }

	[JsonPropertyName("bs")]
	public virtual int? BatteryStatus { get; set; }

	[JsonPropertyName("cog")]
	public virtual int? CourseOverGround { get; set; }

	[JsonPropertyName("conn")]
	public virtual string? ConnectionType { get; set; }

	[JsonPropertyName("created_at")]
	public virtual long? CreatedAt { get; set; }

	[JsonPropertyName("lat")]
	public virtual decimal? Latitude { get; set; }

	[JsonPropertyName("lon")]
	public virtual decimal? Longitude { get; set; }

	[JsonPropertyName("rad")]
	public virtual long? RadiusAroundRegion { get; set; }

	[JsonPropertyName("t")]
	public virtual string? Trigger { get; set; }

	[JsonPropertyName("m")]
	public virtual int? MonitoringMode { get; set; }

	[JsonPropertyName("tid")]
	public virtual string? TrackerID { get; set; }

	[JsonPropertyName("topic")]
	public virtual string? Topic { get; set; }

	[JsonPropertyName("tst")]
	public virtual long? Timestamp { get; set; }

	[JsonPropertyName("vac")]
	public virtual int? VerticalAccuracy { get; set; }

	[JsonPropertyName("vel")]
	public virtual int? Velocity { get; set; }

	[JsonPropertyName("p")]
	public virtual float? Pressure { get; set; }

	[JsonPropertyName("poi")]
	public virtual string? PointOfInterest { get; set; }

	[JsonPropertyName("tag")]
	public virtual string? Tag { get; set; }

	public virtual string? InRegions => SubObjectAsString("inregions");

	public virtual string? InRegionIds => SubObjectAsString("inrids");

	[JsonExtensionData, Ignore]
	public virtual Dictionary<string, JsonElement>? ExtensionData { get; set; }

	public virtual string? SubObjectAsString(string key) {

		if(ExtensionData == null) {

			return null;

		}

		if(!ExtensionData.TryGetValue(key, out JsonElement element)) {

			return null;

		}

		if(element.ValueKind == JsonValueKind.Null) {

			return null;

		}

		return element.GetRawText();

	}

}