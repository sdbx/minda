import React from 'react';
import styled from 'styled-components';
import NavigationBar from '../components/NavigationBar';

const WrapperStyle = styled.div`
    margin-top: 100px;
`;

const MainLayout: React.FC = ({ children }) => {
    return (
        <WrapperStyle>
            <NavigationBar />
            {children}
        </WrapperStyle>
    );
};

export default MainLayout;
