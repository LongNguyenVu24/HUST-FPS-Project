using System.Collections;
using System.Collections.Generic;
using Player.WeaponHandler;
using UnityEngine;
using TMPro;
using System.Diagnostics;

public class PlayerWeapons : MonoBehaviour
{
    public static PlayerWeapons Instance;

    [SerializeField] private BaseGunController gunController;
    [SerializeField] private TextMeshProUGUI AmmoText;
    [SerializeField] private GameObject weaponsHolder;

    [HideInInspector] public List<GunScript> _guns;

    public int random = 30;
    private bool hasPrimaryGun = true;

    public GunScript currentActiveGun;
    public List<GunID> equippedGuns = new List<GunID>() // TODO: Sync this list with guns equipped from Guns Menu
    {
        GunID.AK47_02,
        GunID.GLOCK_01,
    };

    #region Monobehavior

    private void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            Instance = this; 
        } 
    }


    private void Start()
    {
        //EnableEquippedGuns();
        //ICommon.CleanupUnEquippedGuns();
        //ICommon.EnableEquippedGuns(equippedGuns);
        GetEquippedGun();
        _guns = ICommon.GetActiveGunHolders();
        EquipGun(0, true);
        gunController.ManualStart();
    }

    private void Update()
    {
        UpdateCurrentAmmo();
    }
    #endregion

    #region Others
    private void UpdateCurrentAmmo()
    {
        if (AmmoText != null && currentActiveGun != null)
        {
            AmmoText.text = $"{currentActiveGun._currentAmmoInClip}/{currentActiveGun._ammoInReserve}";
        }
    }

    // private void EnableEquippedGuns()
    // {
    //     if (equippedGuns.Count > 2)
    //     {
    //         Debug.LogWarning("Equipped guns count is not expected to be more than 2.");
    //     }

    //     foreach (var gunID in equippedGuns)
    //     {
    //         Debug.LogWarning(gunID);
    //         ICommon.EnableGun(gunID);
    //     }
    // }
    #endregion

    #region Actions
    int prevEquippedIndex = -1;
    public void EquipGun(int slotIndex, bool atIntro = false)
{
    if (!hasPrimaryGun && !atIntro) { return; }
    if (prevEquippedIndex == slotIndex) { return; }
    prevEquippedIndex = slotIndex;
    if (gunController._isReloading) { return; } // TODO: able to switch gun to stop/skip reloading
    if (!atIntro)
    {
        gunController.GunSwitchAnimation();
    }
    for (int i = 0; i < _guns.Count; i++)
    {
        UnityEngine.Debug.LogError("Counttt: " + _guns.Count);
        var gun = _guns[i];
        if (i == slotIndex)
        {
            UnityEngine.Debug.LogError(gun.GunID);
            gun.transform.gameObject.SetActive(true);
            currentActiveGun = gun;
        }
        else
        {
            if (gun != null && gun.transform != null) // Check if the gun reference and transform are not null
            {
                gun.transform.gameObject.SetActive(false);
            }
            else
            {
                // Handle the case where the gun reference or transform is null
                // For example, you could remove the gun from the list
                UnityEngine.Debug.LogError("Gun reference or transform is null at index: " + i);
                _guns.RemoveAt(i);
                i--; // Adjust the index to account for the removed element
            }
        }
    }
}
    #endregion

    #region Get Equipped Gun
    private void GetEquippedGun()
    {
        int gunIndexInDB =-1;
        int gunSkinIndexInDB = -1;
        GunDatabase gunDB = gunController.gunDB;
        string gunName = PlayerPrefs.GetString("GunSelected", "");
        string gunTypeSelected = PlayerPrefs.GetString("TypeSelected");
        UnityEngine.Debug.Log("Gun name check:" + gunName != "");
        if(gunName != "")
        {   
            if (gunTypeSelected == "GunNoSkin")
            {
                for(int i =0; i< gunDB.gunCount; i++)
                {
                    UnityEngine.Debug.LogWarning("Sung trong db: " + gunDB.GetGunAttribute(i).gunObject.ToString());
                    if (gunDB.GetGunAttribute(i).gunObject.ToString() == gunName) //Could have just used index but .. the Gun Menu guy decided to give us gun name
                    {
                        UnityEngine.Debug.LogWarning(i);
                        gunIndexInDB = i;    
                    }
                }
            } else
            {
                for (int i = 0; i < gunDB.gunCount; i++)
                {
                    for (int j = 0; j < gunDB.GetGunAttribute(i).gunSkin.Length; j++)
                    {
                        UnityEngine.Debug.LogWarning("Sung trong db: " + gunDB.GetGunAttribute(i).gunSkin[j].ToString());
                        if (gunDB.GetGunAttribute(i).gunSkin[j].ToString() == gunName) //Could have just used index but .. the Gun Menu guy decided to give us gun name
                        {
                            gunIndexInDB = i;
                            gunSkinIndexInDB = j;
                        }
                    }
                }
            }
            UnityEngine.Debug.Log(gunIndexInDB + gunSkinIndexInDB);


        }
        else
        {
            //Handle no gun equipped
        }

        if(gunIndexInDB != -1)
        {
            List<GunScript> gunsHolder = ICommon.GetLoadedGunHolders();
            for (int i = 0; i < gunsHolder.Count; i++)
            {
                if (gunTypeSelected == "GunNoSkin")
                {
                    if(gunsHolder[i].GunType == gunDB.GetGunAttribute(gunIndexInDB).gunType)
                    {
                        ICommon.ActiveGunHolder(gunsHolder[i]);
                        gunsHolder[i].SpawnGun(gunDB.GetGunAttribute(gunIndexInDB).gunObject);
                        gunsHolder[i].LoadGunStats(gunDB.GetGunAttribute(gunIndexInDB));
                    }
                }else
                {
                    UnityEngine.Debug.Log("GunSkinSelected: " + (gunDB.GetGunAttribute(gunIndexInDB).gunSkin[gunSkinIndexInDB].ToString()));
                    if (gunsHolder[i].GunType == gunDB.GetGunAttribute(gunIndexInDB).gunType)
                    {
                        ICommon.ActiveGunHolder(gunsHolder[i]);
                        gunsHolder[i].SpawnGun(gunDB.GetGunAttribute(gunIndexInDB).gunSkin[gunSkinIndexInDB]);
                        gunsHolder[i].LoadGunStats(gunDB.GetGunAttribute(gunIndexInDB));
                    }
                }
            }
        }else
        {
            hasPrimaryGun = false;
            UnityEngine.Debug.LogError("equipped gun not found in DB(should NOT happen) or Player haven't equipped any gun");
        }
    }
    #endregion
}

