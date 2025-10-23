using Overwolf.CFCore.Base.Creation.Models;
using Overwolf.CFCore.Base.Creation.Models.Enums;
using Overwolf.CFCore.Base.Api.Models.Enums;

namespace Assets.Scripts.Utils {
  public static class CreationUtils  {

    public enum CombinedCreationStatus {
      Failed = 1,
      Rejected = 2,
      Canceled = 3,
      Pending = 4,
      Updating = 5,
      Approved = 6,
    }

    // Start is called before the first frame update
    public static CombinedCreationStatus GetCombinedCreationStatus(CreationItem creation) {

      if (creation.Revision == null && creation.Mod.LatestFile == null) {
        return CombinedCreationStatus.Rejected;
      } else if (creation.Revision != null) {
        switch (creation.Revision.Status) {
          case RevisionStatus.Updating:
            return CombinedCreationStatus.Updating;
          case RevisionStatus.Zipping:
          case RevisionStatus.Pending:
            return CombinedCreationStatus.Pending;
          case RevisionStatus.Canceled:
            return CombinedCreationStatus.Canceled;
          case RevisionStatus.Failed:
            return CombinedCreationStatus.Failed;
        }
      }

      switch (creation.Mod.LatestFile.FileStatus) {
        case FileStatus.Processing:
        case FileStatus.UnderReview:
        case FileStatus.Testing:
        case FileStatus.ReadyForReview:
        case FileStatus.AwaitingPublishing:
          return CombinedCreationStatus.Pending;
        case FileStatus.Approved:
          return CombinedCreationStatus.Approved;
        default:
          return CombinedCreationStatus.Rejected;
      }
    }
  }
}
