import { Outlet } from "react-router-dom";
import Navbar from "@/components/Navbar/Navbar";
import styles from "./layout.module.css";
 
function Layout() {
  return (
    <>
      <Navbar />
      <main className={styles.main}>
        <Outlet />
      </main>
    </>
  );
}
 
export default Layout;