#include "stdafx.h"

#include "EntryPoint.h"
#include "Common.h"
#include "Settings.h"
#include "StrobedVehiclesPool.h"

using namespace EmergencyStrobes;
using namespace System::Windows::Forms;
using namespace System::Threading;

void EntryPoint::Main()
{
	Settings::InitializeINIFile();

	while (Game::IsLoading)
		GameFiber::Yield();

	Game::LogTrivial("Emergency Strobes v" + Common::GetCurrentVersion() + " loaded!");

	if (Settings::EnableAIStrobes)
	{
		StrobedVehiclesPool::Initialize();
	}

	manualActivate = false;
	autoActivate = false;
	finishedHeadlightsFlash = true;
	PlayerVehicle = Game::LocalPlayer->Character->CurrentVehicle;
	Game::LocalPlayer;
	if (Settings::AreHeadlightsEnabled) 
	{
		GameFiber::StartNew(gcnew ThreadStart(&EntryPoint::HeadlightsController), "Headlights Fiber");
	}

	if (Settings::AreIndicatorLightsEnabled)
	{
		GameFiber::StartNew(gcnew ThreadStart(&EntryPoint::IndicatorLightsController), "Indicator lights Fiber");
	}

	while (true)
	{
		GameFiber::Yield();

		Vehicle^ previousPlayerVehicle = PlayerVehicle;

		if (Game::LocalPlayer->Character->IsInAnyVehicle(false)) 
		{
			PlayerVehicle = Game::LocalPlayer->Character->CurrentVehicle;
		}
		else if (Settings::StayOnWhenExitVehicle)
		{
			PlayerVehicle = Game::LocalPlayer->Character->LastVehicle;
			if (PlayerVehicle == nullptr)
			{
				Vehicle^ vehTryingToEnter = Game::LocalPlayer->Character->VehicleTryingToEnter;
				if (previousPlayerVehicle != nullptr && vehTryingToEnter == previousPlayerVehicle)
				{
					PlayerVehicle = vehTryingToEnter;
				}
			}
		}
		else if(!Settings::StayOnWhenExitVehicle && PlayerVehicle != nullptr)
		{
			ResetPlayerVehicleLights();
			PlayerVehicle = nullptr;
		}
		else
		{
			PlayerVehicle = nullptr;
		}

		canPlayerVehBeUsed = PlayerVehicle != nullptr && EntityExtensions::Exists(PlayerVehicle) && (Settings::UseInAllVehicles || (PlayerVehicle->HasSiren && !Settings::ExcludedModels->Contains(PlayerVehicle->Model)));

		if (Settings::ModifierKey == Keys::None)
		{
			if (!autoActivate && Game::IsKeyDown(Settings::ToggleKey))
				manualActivate = !manualActivate;
		}
		else
		{
			if (!autoActivate && Game::IsKeyDownRightNow(Settings::ModifierKey) && Game::IsKeyDown(Settings::ToggleKey))
				manualActivate = !manualActivate;
		}

		if (!autoActivate && Settings::ToggleWithSirenOn && PlayerVehicle != nullptr && EntityExtensions::Exists(PlayerVehicle) && PlayerVehicle->IsSirenOn)
		{
			autoActivate = true;
		}
		else if(autoActivate  && PlayerVehicle != nullptr && EntityExtensions::Exists(PlayerVehicle) && !PlayerVehicle->IsSirenOn)
		{
			autoActivate = false;
		}

		if (autoActivate && PlayerVehicle != nullptr && EntityExtensions::Exists(PlayerVehicle) && PlayerVehicle->IsSirenOn)
		{
			if (Settings::ModifierKey == Keys::None)
			{
				if (Game::IsKeyDown(Settings::ToggleKey))
					disableAutoActivateManually = !disableAutoActivateManually;
			}
			else
			{
				if (Game::IsKeyDownRightNow(Settings::ModifierKey) && Game::IsKeyDown(Settings::ToggleKey))
					disableAutoActivateManually = !disableAutoActivateManually;
			}
		}

		if (disableAutoActivateManually && PlayerVehicle != nullptr && EntityExtensions::Exists(PlayerVehicle) && !PlayerVehicle->IsSirenOn)
		{
			disableAutoActivateManually = false;
		}


		if (!autoActivate && !manualActivate && finishedHeadlightsFlash && PlayerVehicle != nullptr && EntityExtensions::Exists(PlayerVehicle))
		{
			isLeftHeadlightBroken = Common::IsVehicleLeftHeadlightBroken(PlayerVehicle);
			isRightHeadlightBroken = Common::IsVehicleRightHeadlightBroken(PlayerVehicle);
		}
	}
}

void EntryPoint::HeadlightsController()
{
	unsigned int headlightsInt = 0;
	while (true)
	{
		GameFiber::Yield();

		if (canPlayerVehBeUsed)
		{
			if ((autoActivate || manualActivate) && !disableAutoActivateManually)
			{
				finishedHeadlightsFlash = false;

				isLeftHeadlightDeformated = Common::GetVehicleDeformationAtFrontLeft(PlayerVehicle).Length() > 0.01225f;
				isRightHeadlightDeformated = Common::GetVehicleDeformationAtFrontRight(PlayerVehicle).Length() > 0.01225f;

				NativeFunction::CallByName<unsigned int>((String ^)"SET_VEHICLE_LIGHT_MULTIPLIER", gcnew array<NativeArgument^> { PlayerVehicle, Settings::HeadlightsBrightness });
				NativeFunction::CallByName<unsigned int>((String ^)"SET_VEHICLE_LIGHTS", gcnew array<NativeArgument^> { PlayerVehicle, 2 });
				if (headlightsInt % 2 == 0)
				{
					Common::SetVehicleLeftHeadlightBroken(PlayerVehicle, true);

					if (!isRightHeadlightBroken && !isRightHeadlightDeformated)
						Common::SetVehicleRightHeadlightBroken(PlayerVehicle, false);
				}
				else
				{
					if (!isLeftHeadlightBroken && !isLeftHeadlightDeformated)
						Common::SetVehicleLeftHeadlightBroken(PlayerVehicle, false);

					Common::SetVehicleRightHeadlightBroken(PlayerVehicle, true);
				}
				headlightsInt++;
				PlayerVehicle->IsEngineOn = true;
				GameFiber::Sleep(Settings::HeadlightsInterval);
			}
			else if (PlayerVehicle != nullptr && EntityExtensions::Exists(PlayerVehicle) && headlightsInt != 0)
			{
				headlightsInt = 0;

				if (!isLeftHeadlightBroken && !isLeftHeadlightDeformated)
					Common::SetVehicleLeftHeadlightBroken(PlayerVehicle, false);
				if (!isRightHeadlightBroken && !isRightHeadlightDeformated)
					Common::SetVehicleRightHeadlightBroken(PlayerVehicle, false);

				NativeFunction::CallByName<unsigned int>((String ^)"SET_VEHICLE_LIGHTS", gcnew array<NativeArgument^> { PlayerVehicle, 0 });
				NativeFunction::CallByName<unsigned int>((String ^)"SET_VEHICLE_LIGHT_MULTIPLIER", gcnew array<NativeArgument^> { PlayerVehicle, 1.0f });

				finishedHeadlightsFlash = true;
			}
		}
	}
}


void EntryPoint::IndicatorLightsController()
{
	unsigned int indicatorlightsInt = 0;
	while (true)
	{
		GameFiber::Yield();

		if (canPlayerVehBeUsed)
		{
			if ((autoActivate || manualActivate) && !disableAutoActivateManually)
			{
				if (indicatorlightsInt % 2 == 0)
				{
					PlayerVehicle->IndicatorLightsStatus = VehicleIndicatorLightsStatus::LeftOnly;
				}
				else
				{
					PlayerVehicle->IndicatorLightsStatus = VehicleIndicatorLightsStatus::RightOnly;
				}
				indicatorlightsInt++;
				GameFiber::Sleep(Settings::IndicatorLightsInterval);
			}
			else if (PlayerVehicle != nullptr && EntityExtensions::Exists(PlayerVehicle) && indicatorlightsInt != 0)
			{
				indicatorlightsInt = 0;
				PlayerVehicle->IndicatorLightsStatus = VehicleIndicatorLightsStatus::Off;
			}

		}
	}
}


void EntryPoint::ResetPlayerVehicleLights()
{
	if (EntityExtensions::Exists(PlayerVehicle))
	{
		PlayerVehicle->IndicatorLightsStatus = VehicleIndicatorLightsStatus::Off;

		if (!isLeftHeadlightBroken && !isRightHeadlightDeformated)
			Common::SetVehicleLeftHeadlightBroken(PlayerVehicle, false);

		if (!isRightHeadlightBroken && !isRightHeadlightDeformated)
			Common::SetVehicleRightHeadlightBroken(PlayerVehicle, false);

		NativeFunction::CallByName<unsigned int>((String ^)"SET_VEHICLE_LIGHTS", gcnew array<NativeArgument^> { PlayerVehicle, 0 });
		NativeFunction::CallByName<unsigned int>((String ^)"SET_VEHICLE_LIGHT_MULTIPLIER", gcnew array<NativeArgument^> { PlayerVehicle, 1.0f });
	}
}
