namespace KuroAcad
{
    internal class VinxDemo
    {
        internal int ChieuDai { get; set; }
        internal int ChieuRong { get; set; }

        internal double KQ_DienTich;
        internal double KQ_ChuVi;

        internal VinxDemo()
        {
        }

        internal VinxDemo(int Dai, int Rong)
        {
            this.ChieuDai = Dai;
            this.ChieuRong = Rong;
        }

        internal void TinhToan()
        {
            this.DienTich();
            this.KQ_ChuVi = this.ChuVi();
        }

        /// <summary>
        /// Hàm tính diện tích
        /// </summary>
        private void DienTich()
        {
            this.KQ_DienTich = this.ChieuDai * this.ChieuRong;
        }

        /// <summary>
        /// Hàm tính chu vi
        /// </summary>
        /// <returns></returns>
        private double ChuVi()
        {
            return (this.ChieuDai + this.ChieuRong) * 2;
        }


        /// <summary>
        /// Hàm xử lý nội dung
        /// </summary>
        /// <param name="gt1">Giá trị đại diện cho ....</param>
        /// <param name="gt2">Giá trị phụ thuộc ...</param>
        internal void Xulynoidung(string gt1, string gt2)
        {
            //Xử lý nội dung
        }
    }
}
