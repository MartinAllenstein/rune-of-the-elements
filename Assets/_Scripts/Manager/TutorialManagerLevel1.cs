using System;
using UnityEngine;

public class TutorialManagerLevel1 : MonoBehaviour
{
    
    [Header("UI Pop-Ups")]
    [SerializeField] private GameObject[] popUps;
    

    [Header("Counter References")]
    [SerializeField] private StoveCounter alembicCounter;
    [SerializeField] private CauldronCounter cauldronCounter;
    [SerializeField] private EmptyTower emptyTower;

    [Header("Ingredient SOs")]
    [SerializeField] private KitchenObjectSO ingredient1_Raw;
    [SerializeField] private KitchenObjectSO ingredient1_Cooked;
    [SerializeField] private KitchenObjectSO ingredient1_Burned;
    [SerializeField] private KitchenObjectSO ingredient2_Raw;
    [SerializeField] private KitchenObjectSO ingredient2_Cooked;
    [SerializeField] private KitchenObjectSO ingredient2_Burned;
    [SerializeField] private KitchenObjectSO finalPotion;
    
    private int popUpIndex = -1;
    private float timer;
    private bool waitingForGrab = false;
    
    private void Start()
    {
        foreach (GameObject popUp in popUps)
        {
            popUp.SetActive(false);
        }
    }

    private void Update()
    {
        if (popUpIndex == -1)
        {
            if (GameManager.Instance.IsGamePlaying())
            {
                popUpIndex = 0; // Start Tutorial
                Debug.Log("Start Tutorial");
            }
            else
            {
                return;
            }
        }
        
        if (popUpIndex >= popUps.Length) return; // Tutorial End
        
        bool isProcessingState = (popUpIndex == 2 || popUpIndex == 8);
        
        if (isProcessingState)
        {
            // ถ้าใช่, ให้ซ่อน Pop-up ทั้งหมด
            foreach (GameObject popUp in popUps)
            {
                popUp.SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < popUps.Length; i++)
            {
                popUps[i].SetActive(i == popUpIndex);
            }
        }
        
        
        switch (popUpIndex)
        {
            // --- Ingredient 1 ---
            case 0: // Counter 1
                if (Player.Instance.HasKitchenObject() && Player.Instance.GetKitchenObject().GetKitchenObjectSO() == ingredient1_Raw)
                {
                    popUpIndex++;
                }
                break;

            case 1: // Alembic
                if (alembicCounter.HasKitchenObject() && alembicCounter.GetKitchenObject().GetKitchenObjectSO() == ingredient1_Raw)
                {
                    popUpIndex++;
                    timer = 5f;
                }
                break;

            case 2: // รอ 5 วินาที
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    popUpIndex++;
                    timer = 6f; // ตั้งเวลาสำหรับเช็คว่าไหม้ (6 วินาที)
                    waitingForGrab = true;
                }
                break;

            case 3: // ชี้ที่ Alembic รอให้หยิบ (เช็คทัน/ไม่ทัน)
                if (waitingForGrab)
                {
                    timer -= Time.deltaTime;
                    // กรณีหยิบทัน
                    if (Player.Instance.HasKitchenObject() && Player.Instance.GetKitchenObject().GetKitchenObjectSO() == ingredient1_Cooked)
                    {
                        popUpIndex = 4; // ข้ามไปขั้นตอน "ไปที่ Cauldron"
                        waitingForGrab = false;
                    }
                    // กรณีไหม้ (เวลาหมด หรือ ของบนเตากลายเป็นของไหม้)
                    else if (timer <= 0f || (alembicCounter.HasKitchenObject() && alembicCounter.GetKitchenObject().GetKitchenObjectSO() == ingredient1_Burned))
                    {
                        popUpIndex = 5; // ไปขั้นตอน "ไปที่ถังขยะ"
                        waitingForGrab = false;
                    }
                }
                break;

            case 4: // ชี้ที่ Cauldron (กรณีหยิบทัน)
                if (cauldronCounter.GetKitchenObjectSOList().Contains(ingredient1_Cooked))
                {
                    popUpIndex = 6; // เริ่มขั้นตอนวัตถุดิบ 2
                }
                break;

            case 5: // ชี้ที่ถังขยะ (กรณีไหม้)
                if (!Player.Instance.HasKitchenObject() && !alembicCounter.HasKitchenObject())
                {
                    popUpIndex = 0; // กลับไปเริ่มหยิบวัตถุดิบ 1 ใหม่
                }
                break;

            // --- Ingredient 2 ---
            case 6: // ชี้ที่ Counter วัตถุดิบ 2
                if (Player.Instance.HasKitchenObject() && Player.Instance.GetKitchenObject().GetKitchenObjectSO() == ingredient2_Raw)
                {
                    popUpIndex++;
                }
                break;

            case 7: // ชี้ที่ Alembic
                if (alembicCounter.HasKitchenObject() && alembicCounter.GetKitchenObject().GetKitchenObjectSO() == ingredient2_Raw)
                {
                    popUpIndex++;
                    timer = 5f;
                }
                break;

            case 8: // รอ 5 วินาที
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    popUpIndex++;
                    timer = 6f;
                    waitingForGrab = true;
                }
                break;

            case 9: // ชี้ที่ Alembic รอให้หยิบ
                if (waitingForGrab)
                {
                    timer -= Time.deltaTime;
                    if (Player.Instance.HasKitchenObject() && Player.Instance.GetKitchenObject().GetKitchenObjectSO() == ingredient2_Cooked)
                    {
                        popUpIndex = 10;
                        waitingForGrab = false;
                    }
                    else if (timer <= 0f || (alembicCounter.HasKitchenObject() && alembicCounter.GetKitchenObject().GetKitchenObjectSO() == ingredient2_Burned))
                    {
                        popUpIndex = 11;
                        waitingForGrab = false;
                    }
                }
                break;

            case 10: // ชี้ที่ Cauldron
                if (cauldronCounter.GetKitchenObjectSOList().Count == 2)
                {
                    popUpIndex = 12;
                }
                break;

            case 11: // ชี้ที่ถังขยะ
                if (!Player.Instance.HasKitchenObject() && !alembicCounter.HasKitchenObject())
                {
                    popUpIndex = 6;
                }
                break;

            // --- ขั้นตอนสุดท้าย ---
            case 12: // ชี้ที่ Cauldron (ให้ผสม)
                if (cauldronCounter.HasKitchenObject() && cauldronCounter.GetKitchenObject().GetKitchenObjectSO() == finalPotion)
                {
                    popUpIndex++;
                }
                break;
                
            case 13: // ชี้ที่ Cauldron (ให้หยิบ)
                if (Player.Instance.HasKitchenObject() && Player.Instance.GetKitchenObject().GetKitchenObjectSO() == finalPotion)
                {
                    popUpIndex++;
                }
                break;

            case 14: // ชี้ที่ Tower
                if (emptyTower == null) // ตรวจสอบว่า Tower ถูกสร้างแล้ว (EmptyTower หายไป)
                {
                    foreach (GameObject popUp in popUps)
                    {
                        popUp.SetActive(false);
                    }
                    popUpIndex++; // จบ Tutorial
                    Debug.Log("Tutorial Completed!");
                }
                break;
        }
    }
}