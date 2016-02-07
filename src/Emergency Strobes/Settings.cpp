#include "stdafx.h"

#include "Settings.h"

using namespace EmergencyStrobes;
using namespace System::Windows::Forms;

void Settings::InitializeINIFile() 
{
	INIFile = gcnew InitializationFile("Plugins\\Emergency Strobes.ini");
}

#pragma region General section
Keys Settings::ModifierKey::get()
{
	return INIFile->ReadEnum<Keys>("General", "Modifier key", Keys::LControlKey);
}

Keys Settings::ToggleKey::get()
{
	return INIFile->ReadEnum<Keys>("General", "Manual toggle key", Keys::T);
}

float Settings::HeadlightsBrightness::get()
{
	return INIFile->ReadSingle("General", "Headlights brightness", 20.0f);
}

int Settings::HeadlightsInterval::get()
{
	return INIFile->ReadInt32("General", "Headlights interval", 140);
}

int Settings::IndicatorLightsInterval::get()
{
	return INIFile->ReadInt32("General", "Indicator lights interval", 225);
}
#pragma endregion


#pragma region Player section
bool Settings::ToggleWithSirenOn::get()
{
	return INIFile->ReadBoolean("Player", "Toggle with siren on", true);
}

bool Settings::UseInAllVehicles::get()
{
	return INIFile->ReadBoolean("Player", "Use in all vehicles", false);
}

bool Settings::AreHeadlightsEnabled::get()
{
	return INIFile->ReadBoolean("Player", "Enable headlights", true);
}

bool Settings::AreIndicatorLightsEnabled::get()
{
	return INIFile->ReadBoolean("Player", "Enable indicator lights", true);
}

bool Settings::StayOnWhenExitVehicle::get()
{
	return INIFile->ReadBoolean("Player", "Stay on when exit vehicle", false);
}

List<Model>^ Settings::ExcludedModels::get()
{
	array<String^>^ strs = INIFile->ReadString("Player", "Excluded models", "")->Split(',');
	if (strs == nullptr || strs->Length <= 0)
		return gcnew List<Model>{ };

	return gcnew List<Model>(Array::ConvertAll(strs, gcnew Converter<String^, Model>(stringToModel)));
}
#pragma endregion


#pragma region AI section
bool Settings::EnableAIStrobes::get()
{
	return INIFile->ReadBoolean("AI", "Enable AI emergency vehicle strobes", true);
}

bool Settings::AIStrobesStayOnWithoutDriver::get()
{
	return INIFile->ReadBoolean("AI", "AI strobes stay on without driver", true);
}

bool Settings::AreAIStrobesHeadlightsEnabled::get()
{
	return INIFile->ReadBoolean("AI", "Enable AI headlights", true);
}

bool Settings::AreAIStrobesIndicatorLightsEnabled::get()
{
	return INIFile->ReadBoolean("AI", "Enable AI indicator lights", true);
}

List<Model>^ Settings::ExcludedAIModels::get()
{
	array<String^>^ strs = INIFile->ReadString("AI", "Excluded AI models", "")->Split(',');
	if (strs == nullptr || strs->Length <= 0)
		return gcnew List<Model>{ };

	return gcnew List<Model>(Array::ConvertAll(strs, gcnew Converter<String^, Model>(stringToModel)));
}
#pragma endregion

Model Settings::stringToModel(String^ modelName)
{
	return (Model)modelName;
}