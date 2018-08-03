using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Dofus_Updater
{
    public class Constants
    {
        public Bitmap _BG = Properties.Resources.window6;
        public Bitmap _MAIN_BG = Properties.Resources.fenetre2;
        public Bitmap _PAGINATION = Properties.Resources.pagination;
        public Bitmap _PAGINATION_OVER = Properties.Resources.pagination_over;
        public Bitmap _FERMER_OVER = Properties.Resources.fermer_over;
        public Bitmap _FERMER = Properties.Resources.fermer;
        public Bitmap _REDUIRE_OVER = Properties.Resources.reduire_over;
        public Bitmap _REDUIRE = Properties.Resources.reduire;
        public Bitmap _OPTION_OVER = Properties.Resources.option_over;
        public Bitmap _OPTION = Properties.Resources.option;
        public Bitmap _PREV_OVER = Properties.Resources.prev_over;
        public Bitmap _PREV = Properties.Resources.prev;
        public Bitmap _NEXT_OVER = Properties.Resources.next_over;
        public Bitmap _NEXT = Properties.Resources.next;
        public Bitmap _SITE_OVER = Properties.Resources.site_over;
        public Bitmap _SITE = Properties.Resources.site;
        public Bitmap _FORUM_OVER = Properties.Resources.forum_over;
        public Bitmap _FORUM = Properties.Resources.forum;
        public Bitmap _VOTER_HOVER = Properties.Resources.voter_over;
        public Bitmap _VOTER = Properties.Resources.voter;
        public Bitmap _JOUER_OVER = Properties.Resources.jouer_over;
        public Bitmap _JOUER = Properties.Resources.jouer;

        public void changeThemePourBlanc()
        {
            _BG = Properties.Resources.window_white1;
            _MAIN_BG = Properties.Resources.fenetre_white;
        }

    }
}
