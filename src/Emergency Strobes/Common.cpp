#include "stdafx.h"

#include "Common.h"

using namespace EmergencyStrobes;
using namespace System::Windows::Forms;

void Common::SetVehicleLeftHeadlightBroken(Vehicle^ veh, bool broken)
{
	unsigned char *const address = reinterpret_cast<unsigned char*>(veh->MemoryAddress.ToPointer());

	if (address == nullptr)
	{
		return;
	}

	const unsigned char mask = 1 << 0;

	if (broken)
	{
		*(address + 1916) |= mask;
	}
	else
	{
		*(address + 1916) &= ~mask;
	}
}


void Common::SetVehicleRightHeadlightBroken(Vehicle^ veh, bool broken)
{
	unsigned char *const address = reinterpret_cast<unsigned char*>(veh->MemoryAddress.ToPointer());

	if (address == nullptr)
	{
		return;
	}

	const unsigned char mask = 1 << 1;

	if (broken)
	{
		*(address + 1916) |= mask;
	}
	else
	{
		*(address + 1916) &= ~mask;
	}
}


bool Common::IsVehicleLeftHeadlightBroken(Vehicle^ veh)
{
	array<NativeArgument^>^ arguments = gcnew array<NativeArgument^>{ veh };
	return NativeFunction::CallByHash<bool>(0x5ef77c9add3b11a3, arguments);//_IS_HEADLIGHT_L_BROKEN
}


bool Common::IsVehicleRightHeadlightBroken(Vehicle^ veh)
{
	array<NativeArgument^>^ arguments = gcnew array<NativeArgument^>{ veh };
	return NativeFunction::CallByHash<bool>(0xa7ecb73355eb2f20, arguments);//_IS_HEADLIGHT_R_BROKEN
}


Vector3 Common::GetVehicleDeformationAtFrontRight(Vehicle^ veh)
{
	Vector3 rightPosition = veh->RightPosition;
	Vector3 frontPosition = veh->FrontPosition;

	Vector3 rightPositionOffset = veh->GetPositionOffset(rightPosition);
	Vector3 frontPositionOffset = veh->GetPositionOffset(frontPosition);

	Vector3^ finalRightPositionOffset = gcnew Vector3
	(
		rightPositionOffset.X,
		frontPositionOffset.Y,
		frontPositionOffset.Z
	);

	return
		NativeFunction::CallByName<Vector3>((String^)"GET_VEHICLE_DEFORMATION_AT_POS", gcnew array<NativeArgument^>{ veh, finalRightPositionOffset->X, finalRightPositionOffset->Y, finalRightPositionOffset->Z });
}


Vector3 Common::GetVehicleDeformationAtFrontLeft(Vehicle^ veh)
{
	Vector3 leftPosition = veh->LeftPosition;
	Vector3 frontPosition = veh->FrontPosition;

	Vector3 leftPositionOffset = veh->GetPositionOffset(leftPosition);
	Vector3 frontPositionOffset = veh->GetPositionOffset(frontPosition);

	Vector3^ finalLeftPositionOffset = gcnew Vector3
		(
			leftPosition.X,
			frontPositionOffset.Y,
			frontPositionOffset.Z
		);

	return
		NativeFunction::CallByName<Vector3>((String^)"GET_VEHICLE_DEFORMATION_AT_POS", gcnew array<NativeArgument^>{ veh, finalLeftPositionOffset->X, finalLeftPositionOffset->Y, finalLeftPositionOffset->Z });
}

String^ Common::GetCurrentVersion()
{
	return System::Reflection::Assembly::GetExecutingAssembly()->GetName()->Version->ToString();
}