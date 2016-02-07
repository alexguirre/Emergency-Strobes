#pragma once

using namespace System;
using namespace System::Windows::Forms;
using namespace System::Collections::Generic;
using namespace Rage;
using namespace Rage::Native;

namespace EmergencyStrobes
{
	public ref class Settings
	{
	public:
		static InitializationFile^ INIFile = nullptr;
		static void InitializeINIFile();
		//General
		static property Keys ModifierKey { Keys get(); }
		static property Keys ToggleKey { Keys get(); }
		static property float HeadlightsBrightness { float get(); }
		static property int HeadlightsInterval { int get(); }
		static property int IndicatorLightsInterval { int get(); }

		//Player
		static property bool ToggleWithSirenOn { bool get(); }
		static property bool UseInAllVehicles { bool get(); }
		static property bool AreHeadlightsEnabled { bool get(); }
		static property bool AreIndicatorLightsEnabled { bool get(); }
		static property bool StayOnWhenExitVehicle { bool get(); }
		static property List<Model>^ ExcludedModels { List<Model>^ get(); }

		//AI
		static property bool EnableAIStrobes { bool get(); }
		static property bool AIStrobesStayOnWithoutDriver { bool get(); }
		static property bool AreAIStrobesHeadlightsEnabled { bool get(); }
		static property bool AreAIStrobesIndicatorLightsEnabled { bool get(); }
		static property List<Model>^ ExcludedAIModels { List<Model>^ get(); }

	private:
		static Model stringToModel(String^ modelName);
	};
}
