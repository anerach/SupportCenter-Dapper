using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SC.BL.Domain
{
  public enum TicketState : byte
  {
    Open = 1,
    Answered,
    ClientAnswer,
    Closed
  }
}
