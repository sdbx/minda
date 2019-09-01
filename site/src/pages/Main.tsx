import React from 'react';
import styled from 'styled-components';
import MainLayout from '../layouts/MainLayout';

const WrapperStyle = styled.div`
    text-align: center;
    font-size: 50px;
`;

const Main: React.FC = () => {
    return (
        <MainLayout>
            <WrapperStyle>
                Minda
            </WrapperStyle>
        </MainLayout>
    );
};

export default Main;
