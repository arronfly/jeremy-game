using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class WeaponSystemTests
{
    private GameObject playerGameObject;
    private WeaponSystem weaponSystem;
    private GameObject bulletPrefab;

    [UnitySetUp]
    public IEnumerator UnitySetup()
    {
        playerGameObject = new GameObject("Player");
        weaponSystem = playerGameObject.AddComponent<WeaponSystem>();
        weaponSystem.weapons = new WeaponData[]
        {
            new WeaponData() { weaponName = "突击步枪", damage = 25, fireRate = 0.2f, magazineSize = 30, maxReserveAmmo = 120, spread = 0.05f, pellets = 1, reloadTime = 1.5f, bulletSpeed = 20f },
            new WeaponData() { weaponName = "狙击枪", damage = 80, fireRate = 1.5f, magazineSize = 5, maxReserveAmmo = 20, spread = 0f, pellets = 1, reloadTime = 2.5f, bulletSpeed = 30f },
            new WeaponData() { weaponName = "冲锋枪", damage = 18, fireRate = 0.1f, magazineSize = 35, maxReserveAmmo = 140, spread = 0.1f, pellets = 1, reloadTime = 1.5f, bulletSpeed = 15f },
            new WeaponData() { weaponName = "散弹枪", damage = 15, fireRate = 0.8f, magazineSize = 8, maxReserveAmmo = 32, spread = 0.3f, pellets = 8, reloadTime = 2f, bulletSpeed = 18f }
        };
        bulletPrefab = new GameObject("Bullet");
        bulletPrefab.AddComponent<Bullet>();
        weaponSystem.bulletPrefabs = new GameObject[] { bulletPrefab, bulletPrefab, bulletPrefab, bulletPrefab };
        weaponSystem.firePoint = playerGameObject.transform;

        yield return null;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(playerGameObject);
        Object.DestroyImmediate(bulletPrefab);
    }

    [Test]
    public void SwitchToWeapon1()
    {
        weaponSystem.EquipWeapon(0);
        Assert.AreEqual("突击步枪", weaponSystem.GetCurrentWeaponName());
    }

    [Test]
    public void SwitchToWeapon2()
    {
        weaponSystem.EquipWeapon(1);
        Assert.AreEqual("狙击枪", weaponSystem.GetCurrentWeaponName());
    }

    [Test]
    public void SwitchToWeapon3()
    {
        weaponSystem.EquipWeapon(2);
        Assert.AreEqual("冲锋枪", weaponSystem.GetCurrentWeaponName());
    }

    [Test]
    public void SwitchToWeapon4()
    {
        weaponSystem.EquipWeapon(3);
        Assert.AreEqual("散弹枪", weaponSystem.GetCurrentWeaponName());
    }

    [Test]
    public void FourWeapons_Available()
    {
        Assert.AreEqual(4, weaponSystem.weapons.Length);
    }

    [Test]
    public void EquipWeapon_ResetsAmmoToMagazineSize()
    {
        weaponSystem.EquipWeapon(0);
        Assert.AreEqual(30, weaponSystem.GetCurrentAmmo());
    }

    [Test]
    public void FireRateLimiting_BlocksRapidFire()
    {
        weaponSystem.EquipWeapon(0);
        weaponSystem.TryShoot();
        float firstFireTime = weaponSystem.lastFireTime;
        weaponSystem.TryShoot();
        Assert.AreEqual(firstFireTime, weaponSystem.lastFireTime);
    }

    [Test]
    public void FireRateLimiting_AllowsFireAfterCooldown()
    {
        weaponSystem.EquipWeapon(1);
        weaponSystem.TryShoot();
        Assert.Greater(weaponSystem.lastFireTime, 0f);
    }

    [Test]
    public void MagazineEmpty_TriggersReload()
    {
        weaponSystem.EquipWeapon(1);
        for (int i = 0; i < 5; i++)
        {
            weaponSystem.TryShoot();
        }
        Assert.AreEqual(0, weaponSystem.GetCurrentAmmo());
        Assert.IsTrue(weaponSystem.IsReloading());
    }

    [Test]
    public void Reload_CalledWhenEmpty()
    {
        weaponSystem.EquipWeapon(1);
        for (int i = 0; i < 5; i++)
        {
            weaponSystem.TryShoot();
        }
        Assert.IsTrue(weaponSystem.IsReloading());
    }

    [Test]
    public void ReloadCompletes_CorrectTime()
    {
        weaponSystem.EquipWeapon(0);
        for (int i = 0; i < 30; i++)
        {
            weaponSystem.TryShoot();
        }
        float reloadTime = weaponSystem.weapons[0].reloadTime;
        Assert.AreEqual(1.5f, reloadTime);
    }

    [Test]
    public void WeaponDamage_VariesByType()
    {
        weaponSystem.EquipWeapon(0);
        Assert.AreEqual(25, weaponSystem.weapons[0].damage);
        weaponSystem.EquipWeapon(1);
        Assert.AreEqual(80, weaponSystem.weapons[1].damage);
    }

    [Test]
    public void WeaponFireRate_VariesByType()
    {
        Assert.AreEqual(0.2f, weaponSystem.weapons[0].fireRate);
        Assert.AreEqual(1.5f, weaponSystem.weapons[1].fireRate);
    }

    [Test]
    public void WeaponReloadTime_VariesByType()
    {
        Assert.AreEqual(1.5f, weaponSystem.weapons[0].reloadTime);
        Assert.AreEqual(2.5f, weaponSystem.weapons[2].reloadTime);
    }
}
