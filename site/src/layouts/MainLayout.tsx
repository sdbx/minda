import React from 'react';
import NavigationBar from '../components/NavigationBar';

const MainLayout: React.FC = ({ children }) => {
    return (
        <>
            <NavigationBar/>
            {children}
        </>
    );
};

export default MainLayout;
