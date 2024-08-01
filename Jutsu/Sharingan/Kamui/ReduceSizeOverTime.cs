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

        void Update() {

            if (isSucked)
            {
                elapsedTime += Time.deltaTime;
                float percentageComplete = elapsedTime / 0.2f;

                if (item != null)
                {
                    item.transform.localScale = Vector3.Lerp(item.transform.localScale, minScale, Mathf.SmoothStep(0, 1, percentageComplete));
                }

                else if (creature != null) {
                
                    creature.transform.localScale = Vector3.Lerp(creature.transform.localScale, minScale, Mathf.SmoothStep(0, 1, percentageComplete));

                }

                



                elapsedTime = 0f;

                if (item != null)
                {
                    if (item.transform.localScale == minScale)
                    {


                        isSucked = false;
                        item.Despawn();

                    }
                }

                else if (creature != null)
                {
                    if (creature.transform.localScale == minScale)
                    {


                        isSucked = false;
                        creature.Despawn();

                    }
                }





            }


        }

    }
}