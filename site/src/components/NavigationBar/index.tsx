import React from 'react';
import { Link } from 'react-router-dom';
import styled from 'styled-components';

const NavWrapperStyle = styled.ul`
    list-style-type: none;
    margin: 0;
    padding: 0;

    overflow: hidden;
    background-color: rgba(255, 255, 255, 0);
`;

const NavItemStyle = styled.li`
    float: left;

    a {
      display: block;
      color: #000000;
      text-align: center;
      padding: 20px 16px;
      text-decoration: none;

      &:hover {
        background-color: #eeeeee;
      }
    }

`;

const NavigationBar: React.FC = () => {
    return (
        <NavWrapperStyle>
            <NavItemStyle><Link to={'/'}>LOGO OR NAME</Link></NavItemStyle>
            <NavItemStyle><Link to={'/stat'}>전적</Link></NavItemStyle>
            <NavItemStyle><Link to={'/'}>랭킹</Link></NavItemStyle>
            <NavItemStyle><Link to={'/'}>지금은 배경 투명색</Link></NavItemStyle>
            <br/>
        </NavWrapperStyle>
    )
};

export default NavigationBar;