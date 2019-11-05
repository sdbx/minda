import React from 'react';
import styled from 'styled-components';
import MindaMotto from '../../atoms/MindaMotto';

const WrapperStyle = styled.div`
    min-width: 100%;
    height: 440px;
    background-color: gray;
`;

const MainBackground: React.FC = () => {
    return (
        <WrapperStyle className="flex flex-col justify-center">
            <MindaMotto />
        </WrapperStyle>
    );
};

export default MainBackground;
