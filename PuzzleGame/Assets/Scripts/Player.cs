using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using UnityEngine;
using PuzzleGame.EventSystem;

namespace PuzzleGame
{
    [RequireComponent(typeof(PlayerController))]
    public class Player : MonoBehaviour
    {
        public class InventoryItem
        {
            public InventoryItemDef def;
            public int quantity;
        }

        public PlayerController controller { get; private set; }
        public ReadOnlyCollection<InventoryItem> inventory { get { return _inventory.AsReadOnly(); } }
        public Actor actor { get; private set; }

        List<InventoryItem> _inventory = new List<InventoryItem>();

        private void Awake()
        {
            actor = GetComponent<Actor>();
            controller = GetComponent<PlayerController>();
            gameObject.layer = GameConst.k_playerLayer;
        }

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }

        public void AddToInventory(InventoryItemDef def, int quantity)
        {
            //the inventory is very small, so just brute force everything
            int index = -1, itemQuantity = 0;
            for(int i=0; i< _inventory.Count; i++)
            {
                if(_inventory[i].def == def)
                {
                    _inventory[i].quantity += quantity;
                    itemQuantity = _inventory[i].quantity;
                    index = i;
                    break;
                }
            }

            if(index == -1)
            {
                _inventory.Add(new InventoryItem() { def = def, quantity = quantity });
                itemQuantity = quantity;
                index = _inventory.Count - 1;
            }

            Messenger.Broadcast(M_EventType.ON_INVENTORY_CHANGE, new InventoryChangeEventData(def, index, itemQuantity));
        }
    }
}
