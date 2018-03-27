using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace GameServer
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IGameMasterSvc" in both code and config file together.
    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface IGameMasterSvc
    {
        [OperationContract(IsInitiating = true, IsTerminating = false)]
        [FaultContract(typeof(GameMasterSvcFault))]
        void MeetTheGameMaster(string clientId);

        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void IntroduceToGameMaster(string displayName);

        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void CancelMatch();

        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void MoveBoardPiece(int srcX, int srcY, int destX, int destY);

        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void AttackBoardPiece(int srcX, int srcY, int destX, int destY, int attackerPowerLevel);
    }
}
