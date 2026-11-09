import { useState } from "react";
import { User } from "lucide-react";
import styles from "./Avatar.module.css";

interface AvatarProps {
  src: string | null;
  name: string;
  size?: number;
}

function Avatar({ src, name, size = 36 }: AvatarProps) {
  const [failedSrc, setFailedSrc] = useState<string | null>(null);
  const showImage = src !== null && src !== "" && src !== failedSrc;

  return (
    <span className={styles.avatar} style={{ width: size, height: size }}>
      {showImage ? (
        <img src={src} alt={name} className={styles.image} onError={() => setFailedSrc(src)}/>
      ) : (
        <User size={Math.round(size / 2)} aria-label={name} />
      )}
    </span>
  );
}

export default Avatar;