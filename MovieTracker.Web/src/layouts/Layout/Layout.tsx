import { Outlet } from "react-router-dom";
import Footer from "@/layouts/Footer/Footer";
import Navbar from "@/layouts/Navbar/Navbar";
import styles from "./layout.module.css";
 
function Layout() {
  return (
    <div className={styles.layout}>
      <Navbar />
      <main className={styles.main}>
        <Outlet />
      </main>
      <Footer />
    </div>
  );
}
 
export default Layout;