using UnityEngine;

namespace SceneAssets.Common.TeleOperation
{
	public class FrankaTeleOpRemote : MonoBehaviour
	{
		[SerializeField] FrankaTeleOpReceiver receiver;
		[SerializeField] Transform default_pose;
		[SerializeField] private Vector3 relative_position;
		[SerializeField] private Vector3 relative_forward;
		
		void Start (){
			if (receiver == null){
				receiver = FindObjectOfType<FrankaTeleOpReceiver>();
			}
		}

		void Update (){
			relative_position = this.default_pose.InverseTransformPoint(this.transform.position);
			
			//relative_forward = this.default_pose.InverseTransformDirection(this.transform.up);
			//this.receiver.SetRelativeTargetEulerPose(relative_position, relative_forward);
			
			var local_rot = Quaternion.Inverse(default_pose.transform.rotation) * this.transform.rotation;
			this.receiver.SetRelativeTargetQuaternionPose(relative_position, local_rot);
		}
	}
}
