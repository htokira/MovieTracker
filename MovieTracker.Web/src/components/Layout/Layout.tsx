import { Outlet } from "react-router-dom";
import Navbar from "../Navbar/Navbar";
import styles from "../../styles/layout.module.css";
 
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