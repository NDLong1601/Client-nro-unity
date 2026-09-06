using UnityEngine.Networking;

namespace Game1
{
	public class BypassCertificateHandler : CertificateHandler
	{
		protected override bool ValidateCertificate(byte[] certificateData)
		{
			return true;
		}
	}
}
