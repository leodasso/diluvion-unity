using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Rewired;
using Diluvion;
using Diluvion.Ships;
using Loot;
using DUI;

public class CheatManager : MonoBehaviour
{

    public bool showing = false;

    PlayerManager pManager;
    Inventory playerInventory;
    static CheatManager cheatManager;

    Player player;
    public static CheatManager Get()
    {
        if (cheatManager != null) return cheatManager;
        cheatManager = FindObjectOfType<CheatManager>();
        return cheatManager;
    }

    Player PlayerControl()
    {
        if (player != null) return player;
        player = ReInput.players.GetPlayer(0);
        return player;
    }

    public Bridge PlayerSub()
    {
        return PlayerManager.pBridge;
    }

    CrewManager PlayerCrew()
    {
        if (PlayerSub() == null) return null;
        return PlayerSub().crewManager;
    }

    Hull PlayerHull()
    {
        if (PlayerSub() == null) return null;
        return PlayerSub().GetHull();

    }

    QuestManager QMan()
    {
        if (!QuestManager.Get()) return null;
        return QuestManager.Get();
    }

    Inventory PlayerInventory()
    {
        if (playerInventory != null) return playerInventory;
        playerInventory = PlayerManager.pBridge.GetInventory();
        return playerInventory;
    }

    void Update()
    {
        if (GameManager.DebugMode())
            if (Input.GetKeyDown("k"))
                if (!showing)
                {
                    showing = true;
                    GameManager.Freeze(this.gameObject);
                    UIManager.Create(UIManager.Get().debugWindow);
                }
                else
                {
                    UIManager.Clear<DUIDebug>();
                    GameManager.UnFreeze(this.gameObject);
                    showing = false;
                }
    }


    #region sub switching
    List<Bridge> returnBridges = new List<Bridge>();
    public List<Bridge> GetSubs()
    {
        /*
        foreach (GameObject g in PrefabDicts.Get().GetPrefabsByPrefix("ship_"))
        {
            if (g == null) continue;
            Bridge b = g.GetComponent<Bridge>();
            if (b == null) continue;
            returnBridges.Add(b);
        }
        */
        return returnBridges;
    }



    public void SwitchToSub(Bridge sub)
    {
        //PlayerManager.Get().SwitchSubs(sub); //Switches to the input sub
    }
    #endregion

    #region Hull Manipulation

    public void SetHealth(float percentage)
    {
        PlayerHull().SetPercentageHP(percentage);
    }

    public void SetInvulnerable(bool invul)
    {
        PlayerHull().Invincibility(invul);
    }

    float GetCrushDepth()
    {
        return PlayerHull().testDepth;
    }

    public void AddCrushDepth(float add)
    {
        PlayerHull().testDepth = GetCrushDepth() + add;
    }

    #endregion

    #region crew Manipulation

    public void AddRandomCrew()
    {
        //PlayerCrew().AddRandomCrewman();
    }


    #endregion

    #region scene Manipulation

    public void LoadScene(string sceneToload)
    {
         //StageManager.Get().LoadDebugScene(sceneToload); TODO
    }
    public void LoadZone(Zones zoneToLoad)
    {
        //StageManager.Get().ResetPersistables();
        //StageManager.Get().LoadNextZone(zoneToLoad);
    }



    #endregion

    #region inventory manipulation
    public void SetGold(int goldAmount)
    {
        if (PlayerInventory() == null) return;
        PlayerInventory().gold = goldAmount;
    }

    public int GetGold()
    {
        if (PlayerInventory() == null) return 0;
        return PlayerInventory().gold;
    }

    public void AddItems(List<DItem> items)
    {
        if (PlayerInventory() == null) return;
        PlayerInventory().AddItems(items);
    }

    public void AddItem(DItem item)
    {
        if (PlayerInventory() == null) return;
        PlayerInventory().AddItem(item);

    }
    public void AddStack(StackedItem items)
    {
        PlayerInventory().AddItem(items);
    }
    public List<StackedItem> GetItems()
    {
        if (PlayerInventory() == null) return null;
        return PlayerInventory().itemStacks;
    }

    public List<StackedItem> GetKeyItems()
    {
        if (PlayerInventory() == null) return null;
        return null;
        //eturn PlayerInventory().keyItemStacks;
    }

    #region quick items
    public void AddScrap(int amount)
    {
        StackedItem scrapStack = new StackedItem(ItemsGlobal.GetItem("item_scrapMetal"), amount);
        AddStack(scrapStack);
    }

    public void AddTorpedo(int amount)
    {
        StackedItem scrapStack = new StackedItem(ItemsGlobal.GetItem("item_Torpedo"), amount);
        AddStack(scrapStack);
    }

    public void AddFood(int amount)
    {
        StackedItem scrapStack = new StackedItem(ItemsGlobal.GetItem("item food"), amount);
        AddStack(scrapStack);
    }


    public void AddReinforcedPlates(int amount)
    {
        StackedItem scrapStack = new StackedItem(ItemsGlobal.GetItem("item reinforced plates"), amount);
        AddStack(scrapStack);
    }

    public void AddEngineParts(int amount)
    {
        StackedItem scrapStack = new StackedItem(ItemsGlobal.GetItem("item engine parts"), amount);
        AddStack(scrapStack);
    }


    public void RefillAir()
    {
        //PlayerInventory().RefillAir(1, true);
    }
    #endregion
    #endregion

    #region warping

    public void WarpPlayerTo(Vector3 position)
    {
        if (PlayerSub() == null) return;
        PlayerSub().transform.position = position;
    }

    public void WarpPlayerToCheckPoint(CheckPoint point)
    {
        WarpPlayerTo(point.respawnPosition.position);
    }

    public void WarpPlayerToLandmark(LandMark lm)
    {
        WarpPlayerTo(lm.transform.position);
    }

    #endregion

    #region Quest manipulation

    public void QuestWarp()
    {
        //List<Quest> activeQuests = QuestManager.Get().activeQuests;
        /*
        if (activeQuests == null) return;
        if (activeQuests.Count < 1) return;
        Transform questDestination = activeQuests.Last().questDestination;
        if (questDestination == null) return;
        WarpPlayerTo(questDestination.position);
        */

    }

    #endregion;



    #region Maps


    #endregion
}
