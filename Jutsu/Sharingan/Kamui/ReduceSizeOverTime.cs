using UnityEngine;
using ThunderRoad;
namespace Jutsu.Kamui
{
    class ReduceSizeOverTime : MonoBehaviour
    {

        Item item;
        internal bool isSucked;
        private float elapsedTime;
        Vector3 minScale;
        Material kamuiDistortion;
        private float distortionValue;
        Creature creature;
        public void Start() {
            if (this.GetComponent<Item>() != null)
            {
                item = GetComponent<Item>();
            }

            else if (this.GetComponent<Creature>() != null) {

                creature = GetComponent<Creature>();
            
            }

            minScale = new Vector3(0.01f,0.01f,0.01f);


            kamuiDistortion = JutsuEntry.local.kamuiVFX.GetComponentInChildren<MeshRenderer>().materials[0].DeepCopyByExpressionTree();

            distortionValue = 0f;


        }

        internal void ResetSize()
        {
            if (item)
            {
                item.gameObject.transform.localScale = new Vector3(1, 1, 1);
            }

            if (creature)
            {
                creature.transform.localScale = new Vector3(1, 1, 1);
            }
        }

        void Update() {

            if (isSucked)
            {
                elapsedTime += Time.deltaTime;
                float percentageComplete = elapsedTime / 1f;

                if (item != null)
                {
                    item.transform.localScale = Vector3.Lerp(item.transform.localScale, minScale, percentageComplete);
                }

                else if (creature != null) {
                    creature.transform.localScale = Vector3.Lerp(creature.transform.localScale, minScale, percentageComplete);
                }
                
                elapsedTime = 0f;

                if (item != null)
                {
                    if (percentageComplete >= 1f)
                    {
                        isSucked = false;
                        GameObject.Destroy(this);
                    }
                }
                else if (creature != null)
                {
                    if (percentageComplete >= 1f)
                    {
                        isSucked = false;
                        GameObject.Destroy(this);
                    }
                }
            }
        }
    }
}