using Sandbox;
using Sandbox.GameLogic.Modules;
using System.Threading.Tasks;

namespace HideAndSeek;

public class SettingsLoaderComponent : Component
{
	/// <summary>
	/// Loaded settings file (either default of user's).
	/// </summary>
	private GameSettings _settings;

	/// <summary>
	/// MapIdent from the settings file (either default or user's).
	/// </summary>
	[Property] public string MapIdent { get; private set; }
	[Property] public MapInstance Map { get; private set; }

	protected override async Task OnLoad()
	{
		_settings = new GameSettings();
		MapIdent = _settings.MapName;

		if ( FileSystem.Data.FileExists( "Settings/UserSettings.json" ) )
		{
			_settings = FileSystem.Data.ReadJson<GameSettings>( "Settings/UserSettings.json" );

			var info = await Package.Fetch( _settings.MapName, true );
			if ( info != null )
			{
				if ( info.PackageType == Package.Type.Map )
				{
					MapIdent = info.FullIdent;
				}
			}
		}
		Map.MapName = MapIdent;
	}

	protected override void OnUpdate()
	{

	}
}
