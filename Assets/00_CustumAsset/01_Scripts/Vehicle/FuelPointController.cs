using System.Collections;
using System.Collections.Generic;
using NWH.VehiclePhysics;
using RGSK;
using UnityEngine;
using UnityEngine.UI;

public class FuelPointController : MonoBehaviour
{
    private VehicleController       vControl;
    private Image                   fuelAmountImg;

    private float fullAmountAlpha   = 125.0f / 255.0f;
    private float fullAlpha         = 1.0f;
    private float emptyAmountAlpha  = 0.0f;

    private bool bSwitchIcon        = true;
    private bool bFuelFull          = true;

    // Start is called before the first frame update
    void Start()
    {
        if (vControl == null)
            vControl = GetComponent<VehicleController>();

        if (fuelAmountImg == null)
			fuelAmountImg = RaceUI.instance.FuelBar;

		vControl.fuel.useFuel = (RaceManager.instance._raceType == RaceManager.RaceType.FuelLimit) ?
            RaceManager.instance.useFuel_Limit : RaceManager.instance.useFuel_Default;

        vControl.fuel.capacity = (RaceManager.instance._raceType == RaceManager.RaceType.FuelLimit) ?
            RaceManager.instance.capacity_Limit : RaceManager.instance.capacity_Default;

        vControl.fuel.amount = (RaceManager.instance._raceType == RaceManager.RaceType.FuelLimit) ?
            RaceManager.instance.amount_Limit : RaceManager.instance.amount_Default;

        vControl.fuel.efficiency = (RaceManager.instance._raceType == RaceManager.RaceType.FuelLimit) ?
            RaceManager.instance.efficiency_Limit : RaceManager.instance.efficiency_Default;
    }

    // Update is called once per frame
    void Update()
    {
        if (vControl.fuel.useFuel == true)
        {
            if (fuelAmountImg.enabled == false)
                fuelAmountImg.enabled = true;

            fuelAmountImg.fillAmount = vControl.fuel.FuelPercentage;

            if (vControl.fuel.FuelPercentage < 0.3f && bFuelFull == true)
            {
                bFuelFull = false;
                StartCoroutine(FlashIcon());
            }
        }
        else
        {
            FuelFull();

            if (fuelAmountImg.enabled == true)
                fuelAmountImg.enabled = false;
        }
    }

    IEnumerator FlashIcon()
    {
		if (vControl.fuel.FuelPercentage >= 0.3f)
		{
			bFuelFull = true;
			FuelFull();
			yield break;
		}

        if (bSwitchIcon == true)
        {
            FuelEmpty();
            bSwitchIcon = false;
        }

        else if (bSwitchIcon == false)
        {
            FuelFull();
            bSwitchIcon = true;
        }

        yield return new WaitForSeconds(0.3f);

        StartCoroutine(FlashIcon());
    }

    private void FuelEmpty()
    {
        Color fuelColor         = fuelAmountImg.color;
        fuelColor.a             = emptyAmountAlpha;
        fuelAmountImg.color     = fuelColor;
    }

    private void FuelFull()
    {
        Color fuelColor         = fuelAmountImg.color;
        fuelColor.a             = fullAmountAlpha;
        fuelAmountImg.color     = fuelColor;
    }
}
