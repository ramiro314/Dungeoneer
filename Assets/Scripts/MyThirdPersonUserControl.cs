/*
Copia modificada de ThirdPersonUserControl de los StandardAssets de Unity.
Extiende de NetworkBehaviour para soportar operaciones de red.
Tambien parte de las mecanicas de juego fueron incluidas.
*/
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityStandardAssets.Utility;

namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof (MyThirdPersonCharacter))]
    public class MyThirdPersonUserControl : NetworkBehaviour
    {
        public MyThirdPersonCharacter m_Character; // A reference to the ThirdPersonCharacter on the object
        private Transform m_Cam;                  // A reference to the main camera in the scenes transform
        private Vector3 m_CamForward;             // The current forward direction of the camera
        private Vector3 m_Move;
        private bool m_Jump;                      // the world-relative desired move direction, calculated from the camForward and user input.
        private bool m_Crouch;

        private RawImage _itemImage;

        public Item item;

        [SyncVar]
        public Color color;
        [SyncVar]
        public string playerName;
        [SyncVar]
        public int coins;

        private void Start()
        {
            // Set the color chosen by the player.
            // Box001 represents the object that has the MeshRenderer for the character.
            transform.FindChild("Box001").GetComponent<SkinnedMeshRenderer>().material.color = color;
            coins = 0;

            // The player is not supposed to appear until the countdown ends, lets remove it out of sight.
            transform.position = new Vector3(1000, 1000, 1000);

            // get the third person character ( this should never be null due to require component )
            m_Character = GetComponent<MyThirdPersonCharacter>();
        }

        public override void OnStartLocalPlayer()
        {
            // get the transform of the main camera
            if (Camera.main != null)
            {
                m_Cam = Camera.main.transform;
                m_Cam.GetComponent<MySmoothFollow>().target = transform.FindChild("CamRef").transform;
            }
            else
            {
                Debug.LogWarning(
                    "Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.", gameObject);
                // we use self-relative controls in this case, which probably isn't what the user wants, but hey, we warned them!
            }
            // Get the image object for the items.
            _itemImage = GameObject.Find("ItemImage").GetComponent<RawImage>();
        }

        private void Update()
        {
            if (isLocalPlayer && !NetworkGameManager.sInstance.gameEnded)
            {
                if (!m_Jump)
                {
                    m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
                }
            }

            // This is executed on the sever, and results in a RPC on the client
            CmdMove(m_Move, m_Crouch, m_Jump);
        }


        // Fixed update is called in sync with physics
        private void FixedUpdate()
        {
            if (isLocalPlayer)
            {
                if (!NetworkGameManager.sInstance.gameEnded)
                {
                    // Trigger item usage.
                    if (Input.GetKey(KeyCode.I) && item != null)
                    {
                        item.effect.caster = this;
                        item.effect.UseItem();
                        item = null;
                        _itemImage.enabled = false;
                    }

                    // read inputs
                    float h = CrossPlatformInputManager.GetAxis("Horizontal");
                    float v = CrossPlatformInputManager.GetAxis("Vertical");
                    m_Crouch = Input.GetKey(KeyCode.C);

                    // calculate move direction to pass to character
                    if (m_Cam != null)
                    {
                        // calculate camera relative direction to move:
                        m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
                        m_Move = v * m_CamForward + h * m_Cam.right;
                    }
                    else
                    {
                        // we use world-relative directions in the case of no main camera
                        m_Move = v * Vector3.forward + h * Vector3.right;
                    }
#if !MOBILE_INPUT
                    // walk speed multiplier
                    if (Input.GetKey(KeyCode.LeftShift)) m_Move *= 0.5f;
#endif
                }
                else
                {
                    // If game ended stop all animations
                    m_Move = new Vector3(0, 0, 0);
                    m_Crouch = false;
                }
                // pass all parameters to the character control script
                m_Character.Move(m_Move, m_Crouch, m_Jump);
                m_Jump = false;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            Debug.Log(string.Format("Colided with item {0}", other.name));
            if (other.gameObject.CompareTag("Coin"))
            {
                // Increase coin count and play sound
                coins++;
                AudioSource.PlayClipAtPoint(
                    other.gameObject.GetComponent<AudioSource>().clip,
                    transform.position
                );
                // The colider is nested two levels into the Coin prefab,
                // lets remove the source of the prefab to keep the scene clean.
                Destroy(other.gameObject.transform.parent.gameObject.transform.parent.gameObject);

                // Win condition
                if (coins >= NetworkGameManager.sInstance.coinsToWin)
                {
                    NetworkGameManager.sInstance.RpcEndGame();
                }
            }

            if (other.gameObject.CompareTag("Item"))
            {
                item = other.gameObject.GetComponent<Item>();
                if (isLocalPlayer)
                {
                    _itemImage.texture = item.itemImage;
                    _itemImage.enabled = true;
                }
                // We need to keep the instance alive to not lose the reference, so lets just move the object out of sight.
                other.gameObject.transform.parent.gameObject.transform.parent.transform.position = new Vector3(-1000, -1000, -1000);
            }

            if (other.gameObject.CompareTag("Projectile") && other.GetComponent<StealProjectile>().caster != this)
            {
                coins -= 1;
                other.GetComponent<StealProjectile>().caster.coins += 1;
                other.GetComponent<SphereCollider>().enabled = false;
            }
        }

        [Command]
        void CmdMove(Vector3 move, bool crouch, bool jump)
        {
            RpcMove(move, crouch, jump);
        }


        [ClientRpc]
        void RpcMove(Vector3 move, bool crouch, bool jump)
        {
            if (isLocalPlayer)
                return;

            m_Move = move;
            m_Crouch = crouch;
            m_Jump = jump;
            m_Character.Move(move, crouch, jump);
        }

        [ClientRpc]
        public void RpcSpawn(Vector3 newPosition)
        {
            if (isLocalPlayer)
            {
                // move back to spawn location
                transform.position = newPosition;
            }
        }

        [ClientRpc]
        public void RpcWinMatch()
        {
            if (isLocalPlayer)
            {
                GameObject youWin = GameObject.Find("YouWin");
                youWin.GetComponent<Text>().enabled = true;
            }
        }

        [ClientRpc]
        public void RpcLoseMatch()
        {
            if (isLocalPlayer)
            {
                GameObject youLose = GameObject.Find("YouLose");
                youLose.GetComponent<Text>().enabled = true;
            }
        }
    }
}
