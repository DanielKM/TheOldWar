using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if GAIA_PRO_PRESENT
namespace Gaia
{
 
    public class FloatingPointFixMember : MonoBehaviour
    {

        protected void OnEnable()
        {
            FloatingPointFix.Instance.AddMember(this);
        }

        protected void OnDestroy()
        {
            if (FloatingPointFix.IsActive)
            {
                FloatingPointFix.Instance.RemoveMember(this);
            }
        }


        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}
#endif