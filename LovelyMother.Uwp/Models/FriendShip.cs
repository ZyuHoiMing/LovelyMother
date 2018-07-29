﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LovelyMother.Uwp.Models
{
    public class FriendShip
    {
        public int ID { get; set; }

        public int UserID { get; set; }

        public User User { get; set; }

        public int FriendID { get; set; }

        public User Friend { get; set; }

    }
}
